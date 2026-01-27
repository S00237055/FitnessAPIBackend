using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessAPI.Models;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkoutsController : ControllerBase
    {
        private readonly FitnessAppDbContext _context;

        public WorkoutsController(FitnessAppDbContext context)
        {
            _context = context;
        }

        // GET: api/Workouts/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetWorkoutHistory(int userId)
        {
            return await _context.Workouts
                .Where(w => w.UserId == userId)
                .Include(w => w.WorkoutSets)
                .ThenInclude(ws => ws.Exercise)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }

        // POST: api/Workouts
        [HttpPost]
        public async Task<ActionResult<Workout>> LogWorkout(Workout workout)
        {
            if (workout.Date == null) workout.Date = DateTime.Now;

            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            
            return CreatedAtAction(nameof(GetWorkoutHistory), new { userId = workout.UserId }, workout);
        }
    }
}