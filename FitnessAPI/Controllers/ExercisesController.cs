using FitnessAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisesController : ControllerBase
    {
        private readonly FitnessAppDbContext _context;

        public ExercisesController(FitnessAppDbContext context)
        {
            _context = context;
        }

        // GET: api/Exercises
        // Returns the list of all exercises
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises()
        {
            return await _context.Exercises.ToListAsync();
        }

        // POST: api/Exercises
        // adds a new exercise e.g. "Bench Press"
        [HttpPost]
        public async Task<ActionResult<Exercise>> PostExercise(Exercise exercise)
        {
           
            if (_context.Exercises.Any(e => e.Name == exercise.Name))
            {
                return BadRequest("This exercise already exists.");
            }

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExercises", new { id = exercise.ExerciseId }, exercise);
        }

        // DELETE: api/Exercises/{id}
        // Deletes an exercise by its ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
