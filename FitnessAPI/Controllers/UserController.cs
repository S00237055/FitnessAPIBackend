using Azure.Core;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly FitnessAppDbContext _context;
        public UserController(FitnessAppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // Register new user
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(LoginRequest request)
        {
            byte[] salt = PasswordHelper.CreateSalt();
            byte[] hash = PasswordHelper.HashPassword(request.Password, salt);
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = Convert.ToBase64String(hash),
                PasswordSalt = Convert.ToBase64String(salt)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(newUser);
        }

        //Login
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(LoginRequest request)
        {
            // find user in database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);
            // check if user and password exist
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");

            }

            byte[] dbSalt = Convert.FromBase64String(user.PasswordSalt);
            byte[] dbHash = Convert.FromBase64String(user.PasswordHash);

            bool isValid = PasswordHelper.VerifyPassword(request.Password, dbSalt, dbHash);

            if (!isValid)
            {
                return Unauthorized("Invalid username or password.");
            }

            // return user info
            return Ok(user);
        }

        // expected data for login
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

    }
}
