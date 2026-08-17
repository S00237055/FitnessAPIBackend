using FitnessAPI.Controllers;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessAPI.Tests;

public class FoodLogsControllerTests
{
    private static FoodLogDto SampleFood(int userId, string name = "Porridge") => new FoodLogDto
    {
        UserId = userId,
        FoodName = name,
        Calories = 389,
        ProteinGrams = 16.9,
        CarbsGrams = 66.3,
        FatGrams = 6.9
    };

    [Fact]
    public async Task PostFoodLog_PersistsAllMacronutrientValuesAndATimestamp()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var controller = new FoodLogsController(context).AuthenticatedAs(user.UserId);

        var before = DateTime.Now.AddSeconds(-5);

        var result = await controller.PostFoodLog(SampleFood(user.UserId));

        Assert.IsType<OkObjectResult>(result);

        var stored = await context.FoodLogs.SingleAsync();
        Assert.Equal("Porridge", stored.FoodName);
        Assert.Equal(389, stored.Calories);
        Assert.Equal(16.9, stored.ProteinGrams);
        Assert.Equal(66.3, stored.CarbsGrams);
        Assert.Equal(6.9, stored.FatGrams);

        Assert.NotNull(stored.DateEaten);
        Assert.InRange(stored.DateEaten!.Value, before, DateTime.Now.AddSeconds(5));
    }

   
    [Fact]
    public async Task PostFoodLog_IgnoresTheUserIdInTheBody_AndUsesTheToken()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");

        var controller = new FoodLogsController(context).AuthenticatedAs(alice.UserId);

        await controller.PostFoodLog(SampleFood(bob.UserId, "Injected"));

        var stored = await context.FoodLogs.SingleAsync();
        Assert.Equal(alice.UserId, stored.UserId);
    }

    [Fact]
    public void GetUserFoodLogs_ReturnsOwnEntriesNewestFirstAndForbidsOthers()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");

        TestDatabase.SeedFoodLog(context, alice.UserId, "Older", eatenAt: DateTime.Now.AddDays(-2));
        TestDatabase.SeedFoodLog(context, alice.UserId, "Newer", eatenAt: DateTime.Now);
        TestDatabase.SeedFoodLog(context, bob.UserId, "Bob Food");

        var controller = new FoodLogsController(context).AuthenticatedAs(alice.UserId);

        var own = Assert.IsType<OkObjectResult>(controller.GetUserFoodLogs(alice.UserId));
        var logs = Assert.IsAssignableFrom<IEnumerable<FoodLog>>(own.Value).ToList();

        Assert.Equal(2, logs.Count);            // Bob's entry is not included
        Assert.Equal("Newer", logs[0].FoodName); // newest first

        Assert.IsType<ForbidResult>(controller.GetUserFoodLogs(bob.UserId));
    }
}
