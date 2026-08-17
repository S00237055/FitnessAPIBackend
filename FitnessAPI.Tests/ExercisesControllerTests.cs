using FitnessAPI.Controllers;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessAPI.Tests;

public class ExercisesControllerTests
{
    [Fact]
    public async Task PostExercise_RejectsADuplicateNameAndDoesNotAddASecondRow()
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
        Assert.Equal(1, await context.Exercises.CountAsync());
    }
}
