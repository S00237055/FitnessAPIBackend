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
    public async Task LogWorkout_WithValidSession_ReturnsCreated()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new WorkoutsController(context);

        var result = await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now));

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task LogWorkout_PersistsTheSessionAndAllOfItsSets()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new WorkoutsController(context);

        await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now));

        // The parent and both children must be written together.
        Assert.Equal(1, await context.Workouts.CountAsync());
        Assert.Equal(2, await context.WorkoutSets.CountAsync());
    }

    [Fact]
    public async Task LogWorkout_PreservesTheWeightAndRepsOfEachSet()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new WorkoutsController(context);

        await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now));

        var sets = await context.WorkoutSets.OrderBy(s => s.SetNumber).ToListAsync();
        Assert.Equal(60, sets[0].WeightKg);
        Assert.Equal(10, sets[0].Reps);
        Assert.Equal(65, sets[1].WeightKg);
        Assert.Equal(8, sets[1].Reps);
    }

    [Fact]
    public async Task LogWorkout_WithNoDateSupplied_DefaultsToTheCurrentTime()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new WorkoutsController(context);

        var before = DateTime.Now.AddSeconds(-5);

        await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, date: null));

        var stored = await context.Workouts.SingleAsync();
        Assert.NotNull(stored.Date);
        Assert.InRange(stored.Date!.Value, before, DateTime.Now.AddSeconds(5));
    }

    [Fact]
    public async Task GetWorkoutHistory_ReturnsOnlyTheRequestedUsersSessions()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new WorkoutsController(context);

        await controller.LogWorkout(BuildWorkout(alice.UserId, exercise.ExerciseId, DateTime.Now));
        await controller.LogWorkout(BuildWorkout(bob.UserId, exercise.ExerciseId, DateTime.Now));

        var result = await controller.GetWorkoutHistory(alice.UserId);

        var workouts = Assert.IsAssignableFrom<IEnumerable<Workout>>(result.Value).ToList();
        Assert.Single(workouts);
        Assert.Equal(alice.UserId, workouts[0].UserId);
    }

    [Fact]
    public async Task GetWorkoutHistory_ReturnsSessionsNewestFirst()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new WorkoutsController(context);

        await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now.AddDays(-5)));
        await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now));
        await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now.AddDays(-2)));

        var result = await controller.GetWorkoutHistory(user.UserId);

        var workouts = Assert.IsAssignableFrom<IEnumerable<Workout>>(result.Value).ToList();
        Assert.True(workouts[0].Date >= workouts[1].Date);
        Assert.True(workouts[1].Date >= workouts[2].Date);
    }

    [Fact]
    public async Task GetWorkoutHistory_EagerLoadsSetsAndTheirExercises()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var exercise = TestDatabase.SeedExercise(context, "Deadlift", "back");
        var controller = new WorkoutsController(context);

        await controller.LogWorkout(BuildWorkout(user.UserId, exercise.ExerciseId, DateTime.Now));

        var result = await controller.GetWorkoutHistory(user.UserId);

        var workouts = Assert.IsAssignableFrom<IEnumerable<Workout>>(result.Value).ToList();
        var sets = workouts[0].WorkoutSets.ToList();
        Assert.Equal(2, sets.Count);
        Assert.NotNull(sets[0].Exercise);
        Assert.Equal("Deadlift", sets[0].Exercise!.Name);
    }

    [Fact]
    public async Task GetWorkoutHistory_ForAUserWithNoSessions_ReturnsAnEmptyList()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var controller = new WorkoutsController(context);

        var result = await controller.GetWorkoutHistory(user.UserId);

        var workouts = Assert.IsAssignableFrom<IEnumerable<Workout>>(result.Value);
        Assert.Empty(workouts);
    }
}
