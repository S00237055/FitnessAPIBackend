using Microsoft.AspNetCore.Mvc;
using FitnessAPI.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            
            var foodLog = new FoodLog
            {
                UserId = foodDto.UserId,
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
            
            var logs = _context.FoodLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.DateEaten)
                .ToList();

            return Ok(logs);
        }
    }

   
    public class FoodLogDto
    {
        public int UserId { get; set; }
        public string FoodName { get; set; }
        public int Calories { get; set; }
        public double ProteinGrams { get; set; }
        public double CarbsGrams { get; set; }
        public double FatGrams { get; set; }
    }
}