using FitnessAPI.Controllers;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessAPI.Tests;

public class ExercisesControllerTests
{
    [Fact]
    public async Task GetExercises_ReturnsEveryExerciseInTheCatalogue()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedExercise(context, "Bench Press", "chest");
        TestDatabase.SeedExercise(context, "Deadlift", "back");
        TestDatabase.SeedExercise(context, "Squat", "upper legs");
        var controller = new ExercisesController(context);

        var result = await controller.GetExercises();

        var exercises = Assert.IsAssignableFrom<IEnumerable<Exercise>>(result.Value);
        Assert.Equal(3, exercises.Count());
    }

    [Fact]
    public async Task GetExercises_WithAnEmptyCatalogue_ReturnsAnEmptyList()
    {
        using var context = TestDatabase.Create();
        var controller = new ExercisesController(context);

        var result = await controller.GetExercises();

        var exercises = Assert.IsAssignableFrom<IEnumerable<Exercise>>(result.Value);
        Assert.Empty(exercises);
    }

    [Fact]
    public async Task PostExercise_WithANewName_ReturnsCreated()
    {
        using var context = TestDatabase.Create();
        var controller = new ExercisesController(context);

        var result = await controller.PostExercise(new Exercise
        {
            Name = "Overhead Press",
            BodyPart = "shoulders"
        });

        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(1, await context.Exercises.CountAsync());
    }

    [Fact]
    public async Task PostExercise_WithADuplicateName_ReturnsBadRequest()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedExercise(context, "Bench Press");
        var controller = new ExercisesController(context);

        var result = await controller.PostExercise(new Exercise
        {
            Name = "Bench Press",
            BodyPart = "chest"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostExercise_WithADuplicateName_DoesNotAddASecondRow()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedExercise(context, "Bench Press");
        var controller = new ExercisesController(context);

        await controller.PostExercise(new Exercise { Name = "Bench Press", BodyPart = "chest" });

        Assert.Equal(1, await context.Exercises.CountAsync());
    }

    [Fact]
    public async Task DeleteExercise_WithAnExistingId_ReturnsNoContent()
    {
        using var context = TestDatabase.Create();
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new ExercisesController(context);

        var result = await controller.DeleteExercise(exercise.ExerciseId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteExercise_RemovesTheRowFromTheDatabase()
    {
        using var context = TestDatabase.Create();
        var exercise = TestDatabase.SeedExercise(context);
        var controller = new ExercisesController(context);

        await controller.DeleteExercise(exercise.ExerciseId);

        Assert.Equal(0, await context.Exercises.CountAsync());
    }

    [Fact]
    public async Task DeleteExercise_WithAnUnknownId_ReturnsNotFound()
    {
        using var context = TestDatabase.Create();
        var controller = new ExercisesController(context);

        var result = await controller.DeleteExercise(9999);

        Assert.IsType<NotFoundResult>(result);
    }
}
