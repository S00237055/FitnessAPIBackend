using FitnessAPI.Controllers;
using FitnessAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessAPI.Tests;

public class UserControllerTests
{
    private static UserController CreateController(FitnessAppDbContext context)
        => new UserController(context, new FakeTokenService());

    [Fact]
    public async Task Register_WithNewUsername_ReturnsOkAndAToken()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context);

        var result = await controller.Register(new UserController.RegisterRequest
        {
            Username = "newuser",
            Password = "Password123",
            CurrentWeight = 75,
            GoalType = "Build Muscle"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<AuthResponseDto>(ok.Value);
        Assert.Equal("newuser", dto.Username);
        Assert.False(string.IsNullOrWhiteSpace(dto.Token));
    }

    [Fact]
    public async Task Register_DoesNotStoreThePasswordInPlaintext()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context);

        await controller.Register(new UserController.RegisterRequest
        {
            Username = "secure",
            Password = "Password123"
        });

        var stored = await context.Users.SingleAsync(u => u.Username == "secure");
        Assert.NotEqual("Password123", stored.PasswordHash);

        // The stored hash must still verify against the original password.
        var salt = Convert.FromBase64String(stored.PasswordSalt);
        var hash = Convert.FromBase64String(stored.PasswordHash);
        Assert.True(PasswordHelper.VerifyPassword("Password123", salt, hash));
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedUser(context, "taken");
        var controller = CreateController(context);

        var result = await controller.Register(new UserController.RegisterRequest
        {
            Username = "TAKEN",
            Password = "Password123"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedUser(context, "alice", "Password123");
        var controller = CreateController(context);

        var result = await controller.Login(new UserController.LoginRequest
        {
            Username = "alice",
            Password = "Password123"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<AuthResponseDto>(ok.Value);
        Assert.Equal("alice", dto.Username);
    }

    [Fact]
    public async Task Login_ReturnsTheSameMessage_ForUnknownUserAndWrongPassword()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedUser(context, "alice", "Password123");
        var controller = CreateController(context);

        var unknownUser = await controller.Login(new UserController.LoginRequest
        {
            Username = "nobody",
            Password = "Password123"
        });
        var wrongPassword = await controller.Login(new UserController.LoginRequest
        {
            Username = "alice",
            Password = "WrongPassword"
        });

        var first = Assert.IsType<UnauthorizedObjectResult>(unknownUser.Result);
        var second = Assert.IsType<UnauthorizedObjectResult>(wrongPassword.Result);

        // Differentiated messages would let an attacker enumerate valid usernames.
        Assert.Equal(first.Value, second.Value);
    }

    [Fact]
    public async Task GetUser_MustNotExposePasswordHashOrSalt()
    {
        using var context = TestDatabase.Create();
        var seeded = TestDatabase.SeedUser(context, "alice");
        var controller = CreateController(context).AuthenticatedAs(seeded.UserId);

        var result = await controller.GetUser(seeded.UserId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsNotType<User>(ok.Value);
        Assert.IsType<UserResponseDto>(ok.Value);
    }


    [Fact]
    public async Task GetUser_ForAnotherUsersProfile_ReturnsForbidden()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob");
        var controller = CreateController(context).AuthenticatedAs(alice.UserId);

        var result = await controller.GetUser(bob.UserId);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task UpdateProfile_ForTheAuthenticatedUser_UpdatesTheStoredValues()
    {
        using var context = TestDatabase.Create();
        var seeded = TestDatabase.SeedUser(context, "alice", weight: 80, goal: "Build Muscle");
        var controller = CreateController(context).AuthenticatedAs(seeded.UserId);

        await controller.UpdateProfile(seeded.UserId, new UserController.UpdateProfileRequest
        {
            CurrentWeight = 72.5,
            GoalType = "Lose Weight"
        });

        var stored = await context.Users.SingleAsync(u => u.UserId == seeded.UserId);
        Assert.Equal(72.5, stored.CurrentWeight);
        Assert.Equal("Lose Weight", stored.GoalType);
    }
}
