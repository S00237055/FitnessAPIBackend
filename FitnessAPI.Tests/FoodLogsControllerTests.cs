using FitnessAPI.Controllers;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessAPI.Tests;

public class FoodLogsControllerTests
{
    [Fact]
    public async Task PostFoodLog_WithValidData_ReturnsOk()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var controller = new FoodLogsController(context);

        var result = await controller.PostFoodLog(new FoodLogDto
        {
            UserId = user.UserId,
            FoodName = "Porridge",
            Calories = 389,
            ProteinGrams = 16.9,
            CarbsGrams = 66.3,
            FatGrams = 6.9
        });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task PostFoodLog_PersistsAllMacronutrientValues()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var controller = new FoodLogsController(context);

        await controller.PostFoodLog(new FoodLogDto
        {
            UserId = user.UserId,
            FoodName = "Porridge",
            Calories = 389,
            ProteinGrams = 16.9,
            CarbsGrams = 66.3,
            FatGrams = 6.9
        });

        var stored = await context.FoodLogs.SingleAsync();
        Assert.Equal("Porridge", stored.FoodName);
        Assert.Equal(389, stored.Calories);
        Assert.Equal(16.9, stored.ProteinGrams);
        Assert.Equal(66.3, stored.CarbsGrams);
        Assert.Equal(6.9, stored.FatGrams);
    }

    [Fact]
    public async Task PostFoodLog_SetsTheDateEatenAutomatically()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var controller = new FoodLogsController(context);

        var before = DateTime.Now.AddSeconds(-5);

        await controller.PostFoodLog(new FoodLogDto
        {
            UserId = user.UserId,
            FoodName = "Banana",
            Calories = 89,
            ProteinGrams = 1.1,
            CarbsGrams = 22.8,
            FatGrams = 0.3
        });

        var stored = await context.FoodLogs.SingleAsync();
        Assert.NotNull(stored.DateEaten);
        Assert.InRange(stored.DateEaten!.Value, before, DateTime.Now.AddSeconds(5));
    }

    [Fact]
    public void GetUserFoodLogs_ReturnsOnlyTheRequestedUsersEntries()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");

        TestDatabase.SeedFoodLog(context, alice.UserId, "Alice Food");
        TestDatabase.SeedFoodLog(context, bob.UserId, "Bob Food");
        TestDatabase.SeedFoodLog(context, bob.UserId, "Bob Second Food");

        var controller = new FoodLogsController(context);

        var result = controller.GetUserFoodLogs(alice.UserId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var logs = Assert.IsAssignableFrom<IEnumerable<FoodLog>>(ok.Value).ToList();
        Assert.Single(logs);
        Assert.Equal("Alice Food", logs[0].FoodName);
    }

    [Fact]
    public void GetUserFoodLogs_ReturnsEntriesNewestFirst()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);

        TestDatabase.SeedFoodLog(context, user.UserId, "Oldest", eatenAt: DateTime.Now.AddDays(-3));
        TestDatabase.SeedFoodLog(context, user.UserId, "Newest", eatenAt: DateTime.Now);
        TestDatabase.SeedFoodLog(context, user.UserId, "Middle", eatenAt: DateTime.Now.AddDays(-1));

        var controller = new FoodLogsController(context);

        var result = controller.GetUserFoodLogs(user.UserId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var logs = Assert.IsAssignableFrom<IEnumerable<FoodLog>>(ok.Value).ToList();
        Assert.Equal("Newest", logs[0].FoodName);
        Assert.Equal("Middle", logs[1].FoodName);
        Assert.Equal("Oldest", logs[2].FoodName);
    }

    [Fact]
    public void GetUserFoodLogs_WithNoEntries_ReturnsAnEmptyList()
    {
        using var context = TestDatabase.Create();
        var user = TestDatabase.SeedUser(context);
        var controller = new FoodLogsController(context);

        var result = controller.GetUserFoodLogs(user.UserId);

        var ok = Assert.IsType<OkObjectResult>(result);
        var logs = Assert.IsAssignableFrom<IEnumerable<FoodLog>>(ok.Value);
        Assert.Empty(logs);
    }
}
