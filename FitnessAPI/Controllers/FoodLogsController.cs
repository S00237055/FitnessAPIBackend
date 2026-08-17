using FitnessAPI.Extensions;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FoodLogsController : ControllerBase
    {
        private readonly FitnessAppDbContext _context;

        public FoodLogsController(FitnessAppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostFoodLog(FoodLogDto foodDto)
        {
            var callerId = HttpContext.User.GetUserId();
            if (callerId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(foodDto.FoodName))
            {
                return BadRequest("A food name is required.");
            }

            var foodLog = new FoodLog
            {
                UserId = callerId.Value,
                FoodName = foodDto.FoodName,
                Calories = foodDto.Calories,
                ProteinGrams = foodDto.ProteinGrams,
                CarbsGrams = foodDto.CarbsGrams,
                FatGrams = foodDto.FatGrams,
                DateEaten = DateTime.Now
            };

            _context.FoodLogs.Add(foodLog);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Food logged successfully!", foodLog });
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserFoodLogs(int userId)
        {
            var callerId = HttpContext.User.GetUserId();
            if (callerId == null)
            {
                return Unauthorized();
            }

            if (callerId != userId)
            {
                return Forbid();
            }

            var logs = _context.FoodLogs
                .Where(log => log.UserId == callerId.Value)
                .OrderByDescending(log => log.DateEaten)
                .ToList();

            return Ok(logs);
        }
    }

    public class FoodLogDto
    {
        public int UserId { get; set; }
        public string FoodName { get; set; } = null!;
        public int Calories { get; set; }
        public double ProteinGrams { get; set; }
        public double CarbsGrams { get; set; }
        public double FatGrams { get; set; }
    }
}
