using FitnessAPI.Extensions;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkoutsController : ControllerBase
    {
        private readonly FitnessAppDbContext _context;

        public WorkoutsController(FitnessAppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetWorkouts()
        {
            var callerId = HttpContext.User.GetUserId();
            if (callerId == null)
            {
                return Unauthorized();
            }

            var workouts = await _context.Workouts
                .Where(w => w.UserId == callerId.Value)
                .Include(w => w.WorkoutSets)
                .ThenInclude(s => s.Exercise)
                .OrderByDescending(w => w.Date)
                .ToListAsync();

            return Ok(workouts);
        }

        // GET: api/Workouts/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetWorkoutHistory(int userId)
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

            return await _context.Workouts
                .Where(w => w.UserId == callerId.Value)
                .Include(w => w.WorkoutSets)
                .ThenInclude(ws => ws.Exercise)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }

        // POST: api/Workouts
        [HttpPost]
        public async Task<ActionResult<Workout>> LogWorkout(Workout workout)
        {
            var callerId = HttpContext.User.GetUserId();
            if (callerId == null)
            {
                return Unauthorized();
            }

            workout.UserId = callerId.Value;

            if (workout.Date == null) workout.Date = DateTime.Now;

            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkoutHistory), new { userId = workout.UserId }, workout);
        }
    }
}
