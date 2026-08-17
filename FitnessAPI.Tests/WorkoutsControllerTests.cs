using FitnessAPI.Controllers;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessAPI.Tests;

public class WorkoutsControllerTests
{
    private static Workout BuildWorkout(int userId, int exerciseId, DateTime? date = null)
    {
        return new Workout
        {
            UserId = userId,
            Date = date,
            Notes = "Duration: 42:15",
            WorkoutSets = new List<WorkoutSet>
            {
                new WorkoutSet { ExerciseId = exerciseId, SetNumber = 1, WeightKg = 60, Reps = 10 },
                new WorkoutSet { ExerciseId = exerciseId, SetNumber = 2, WeightKg = 65, Reps = 8 }
            }
        };
    }

    [Fact]
    public async Task LogWorkout_PersistsTheSessionAndAllOfItsSets()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new WorkoutsController(context).AuthenticatedAs(user.UserId);

        var result = await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now));

        Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(1, await context.Workouts.CountAsync());
        var sets = await context.WorkoutSets.OrderBy(s => s.SetNumber).ToListAsync();
        Assert.Equal(2, sets.Count);
        Assert.Equal(60, sets[0].WeightKg);
        Assert.Equal(10, sets[0].Reps);
    }

    [Fact]
    public async Task LogWorkout_IgnoresTheUserIdInTheBody_AndUsesTheToken()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");
        var exercise = TestDatabase.SeedExercise(context);

        var controller = new WorkoutsController(context).AuthenticatedAs(alice.UserId);

        await controller.LogWorkout(BuildWorkout(bob.UserId, exercise.ExerciseId, DateTime.Now));

        var stored = await context.Workouts.SingleAsync();
        Assert.Equal(alice.UserId, stored.UserId);
    }

    [Fact]
    public async Task GetWorkoutHistory_ForAnotherUser_ReturnsForbidden()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");
        var controller = new WorkoutsController(context).AuthenticatedAs(alice.UserId);

        var result = await controller.GetWorkoutHistory(bob.UserId);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetWorkouts_ReturnsOnlyTheAuthenticatedUsersSessions()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");
        var exercise = TestDatabase.SeedExercise(context, "Deadlift", "back");

        await new WorkoutsController(context).AuthenticatedAs(alice.UserId)
            .LogWorkout(BuildWorkout(alice.UserId, exercise.ExerciseId, DateTime.Now));
        await new WorkoutsController(context).AuthenticatedAs(bob.UserId)
            .LogWorkout(BuildWorkout(bob.UserId, exercise.ExerciseId, DateTime.Now));

        var controller = new WorkoutsController(context).AuthenticatedAs(alice.UserId);
        var result = await controller.GetWorkouts();

        var ok = Assert.IsType<OkObjectResult>(result);
        var workouts = Assert.IsAssignableFrom<IEnumerable<Workout>>(ok.Value).ToList();

        Assert.Single(workouts);
        Assert.Equal(alice.UserId, workouts[0].UserId);
        Assert.Equal("Deadlift", workouts[0].WorkoutSets.First().Exercise!.Name);
    }
}
