using System.Security.Claims;
using FitnessAPI.Models;
using FitnessAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAPI.Tests;


public class FakeTokenService : ITokenService
{
    public string CreateToken(User user) => $"test-token-for-user-{user.UserId}";
}

public static class ControllerAuthExtensions
{
    public static T AuthenticatedAs<T>(this T controller, int userId, string username = "testuser")
        where T : ControllerBase
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuthentication");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    public static T Anonymous<T>(this T controller) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        return controller;
    }
}
