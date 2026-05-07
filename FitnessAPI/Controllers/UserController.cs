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
            var users = await _context.Users
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    CurrentWeight = u.CurrentWeight,
                    GoalType = u.GoalType
                })
                .ToListAsync();

            return Ok(users);
        }

        // Register new user
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterRequest request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }
                byte[] salt = PasswordHelper.CreateSalt();
            byte[] hash = PasswordHelper.HashPassword(request.Password, salt);
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = Convert.ToBase64String(hash),
                PasswordSalt = Convert.ToBase64String(salt),
                CurrentWeight = request.CurrentWeight,
                GoalType = request.GoalType
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var userResponse = new UserResponseDto
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                CurrentWeight = newUser.CurrentWeight,
                GoalType = newUser.GoalType
            };

            return Ok(userResponse);
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

            var userResponse = new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                CurrentWeight = user.CurrentWeight,
                GoalType = user.GoalType
            };

            return Ok(userResponse);
        }

        // expected data for login
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public double? CurrentWeight { get; set; }
            public string? GoalType { get; set; }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userResponse = new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                CurrentWeight = user.CurrentWeight,
                GoalType = user.GoalType
            };

            return user;
        }
        public class UpdateProfileRequest
        {
            public double? CurrentWeight { get; set; }
            public string? GoalType { get; set; }
        }

        [HttpPut("{id}/profile")]
        public async Task<ActionResult<UserResponseDto>> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.CurrentWeight = request.CurrentWeight;
            user.GoalType = request.GoalType;

            await _context.SaveChangesAsync();

            var userResponse = new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                CurrentWeight = user.CurrentWeight,
                GoalType = user.GoalType
            };

            return Ok(userResponse);
        }



    }
}
