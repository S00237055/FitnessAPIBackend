using FitnessAPI.Extensions;
using FitnessAPI.Models;
using FitnessAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly FitnessAppDbContext _context;
        private readonly ITokenService _tokenService;

        public UserController(FitnessAppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // Register new user
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

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

            return Ok(new AuthResponseDto
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                CurrentWeight = newUser.CurrentWeight,
                GoalType = newUser.GoalType,
                Token = _tokenService.CreateToken(newUser)
            });
        }

        // Login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);
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

            return Ok(new AuthResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                CurrentWeight = user.CurrentWeight,
                GoalType = user.GoalType,
                Token = _tokenService.CreateToken(user)
            });
        }

        // expected data for login
        public class LoginRequest
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class RegisterRequest
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
            public double? CurrentWeight { get; set; }
            public string? GoalType { get; set; }
        }

        public class UpdateProfileRequest
        {
            public double? CurrentWeight { get; set; }
            public string? GoalType { get; set; }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var callerId = HttpContext.User.GetUserId();
            if (callerId == null)
            {
                return Unauthorized();
            }

            if (callerId != id)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                CurrentWeight = user.CurrentWeight,
                GoalType = user.GoalType
            });
        }

        [Authorize]
        [HttpPut("{id}/profile")]
        public async Task<ActionResult<UserResponseDto>> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            var callerId = HttpContext.User.GetUserId();
            if (callerId == null)
            {
                return Unauthorized();
            }

            if (callerId != id)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.CurrentWeight = request.CurrentWeight;
            user.GoalType = request.GoalType;

            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                CurrentWeight = user.CurrentWeight,
                GoalType = user.GoalType
            });
        }
    }
}
