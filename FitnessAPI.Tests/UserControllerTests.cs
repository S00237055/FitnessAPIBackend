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
    public async Task Register_WithNewUsername_ReturnsOk()
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
        Assert.Equal(75, dto.CurrentWeight);
    }

    [Fact]
    public async Task Register_ReturnsAnAuthenticationToken()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context);

        var result = await controller.Register(new UserController.RegisterRequest
        {
            Username = "newuser",
            Password = "Password123"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<AuthResponseDto>(ok.Value);
        Assert.False(string.IsNullOrWhiteSpace(dto.Token));
    }

    [Fact]
    public async Task Register_PersistsTheUserToTheDatabase()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context);

        await controller.Register(new UserController.RegisterRequest
        {
            Username = "persisted",
            Password = "Password123"
        });

        var stored = await context.Users.SingleOrDefaultAsync(u => u.Username == "persisted");
        Assert.NotNull(stored);
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
        Assert.False(string.IsNullOrEmpty(stored.PasswordSalt));
    }

    [Fact]
    public async Task Register_StoredHashVerifiesAgainstTheOriginalPassword()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context);

        await controller.Register(new UserController.RegisterRequest
        {
            Username = "roundtrip",
            Password = "Password123"
        });

        var stored = await context.Users.SingleAsync(u => u.Username == "roundtrip");
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
            Username = "taken",
            Password = "Password123"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_UsernameComparisonIsCaseInsensitive()
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
    }

    [Fact]
    public async Task Register_DoesNotCreateASecondRecord_WhenUsernameIsTaken()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedUser(context, "taken");
        var controller = CreateController(context);

        await controller.Register(new UserController.RegisterRequest
        {
            Username = "taken",
            Password = "Password123"
        });

        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task Register_WithAnEmptyUsername_ReturnsBadRequest()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context);

        var result = await controller.Register(new UserController.RegisterRequest
        {
            Username = "",
            Password = "Password123"
        });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ------------------------------------------------------------------- Login

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
    public async Task Login_ReturnsAnAuthenticationToken()
    {
        using var context = TestDatabase.Create();
        var seeded = TestDatabase.SeedUser(context, "alice", "Password123");
        var controller = CreateController(context);

        var result = await controller.Login(new UserController.LoginRequest
        {
            Username = "alice",
            Password = "Password123"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<AuthResponseDto>(ok.Value);
        Assert.Equal($"test-token-for-user-{seeded.UserId}", dto.Token);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_ReturnsUnauthorized()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context);

        var result = await controller.Login(new UserController.LoginRequest
        {
            Username = "nobody",
            Password = "Password123"
        });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
    {
        using var context = TestDatabase.Create();
        TestDatabase.SeedUser(context, "alice", "Password123");
        var controller = CreateController(context);

        var result = await controller.Login(new UserController.LoginRequest
        {
            Username = "alice",
            Password = "WrongPassword"
        });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
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

        Assert.Equal(first.Value, second.Value);
    }

    [Fact]
    public async Task GetUser_ForTheAuthenticatedUser_ReturnsTheProfile()
    {
        using var context = TestDatabase.Create();
        var seeded = TestDatabase.SeedUser(context, "alice");
        var controller = CreateController(context).AuthenticatedAs(seeded.UserId);

        var result = await controller.GetUser(seeded.UserId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<UserResponseDto>(ok.Value);
        Assert.Equal("alice", dto.Username);
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
    public async Task GetUser_WithNoAuthenticatedIdentity_ReturnsUnauthorized()
    {
        using var context = TestDatabase.Create();
        var seeded = TestDatabase.SeedUser(context, "alice");
        var controller = CreateController(context).Anonymous();

        var result = await controller.GetUser(seeded.UserId);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetUser_WithUnknownId_ReturnsNotFound()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context).AuthenticatedAs(9999);

        var result = await controller.GetUser(9999);

        Assert.IsType<NotFoundResult>(result.Result);
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

    [Fact]
    public async Task UpdateProfile_ReturnsTheUpdatedProfile()
    {
        using var context = TestDatabase.Create();
        var seeded = TestDatabase.SeedUser(context, "alice");
        var controller = CreateController(context).AuthenticatedAs(seeded.UserId);

        var result = await controller.UpdateProfile(seeded.UserId, new UserController.UpdateProfileRequest
        {
            CurrentWeight = 72.5,
            GoalType = "Lose Weight"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<UserResponseDto>(ok.Value);
        Assert.Equal(72.5, dto.CurrentWeight);
    }

    [Fact]
    public async Task UpdateProfile_ForAnotherUsersProfile_ReturnsForbidden()
    {
        using var context = TestDatabase.Create();
        var alice = TestDatabase.SeedUser(context, "alice");
        var bob = TestDatabase.SeedUser(context, "bob", weight: 90);
        var controller = CreateController(context).AuthenticatedAs(alice.UserId);

        var result = await controller.UpdateProfile(bob.UserId, new UserController.UpdateProfileRequest
        {
            CurrentWeight = 1,
            GoalType = "Tampered"
        });

        Assert.IsType<ForbidResult>(result.Result);

        var stored = await context.Users.SingleAsync(u => u.UserId == bob.UserId);
        Assert.Equal(90, stored.CurrentWeight);
    }

    [Fact]
    public async Task UpdateProfile_WithUnknownUser_ReturnsNotFound()
    {
        using var context = TestDatabase.Create();
        var controller = CreateController(context).AuthenticatedAs(9999);

        var result = await controller.UpdateProfile(9999, new UserController.UpdateProfileRequest
        {
            CurrentWeight = 70,
            GoalType = "Maintain"
        });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
