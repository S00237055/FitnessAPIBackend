using FitnessAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FitnessAPI.Tests;

public class AiControllerTests
{
    private static IConfiguration ConfigurationWithKey(string? key)
    {
        var values = new Dictionary<string, string?>();
        if (key is not null)
        {
            values["GeminiApiKey"] = key;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    [Fact]
    public async Task GetDietAdvice_WithAnEmptyPrompt_ReturnsBadRequest()
    {
        var controller = new AiController(ConfigurationWithKey("test-key"));

        var result = await controller.GetDietAdvice(new AiController.AiRequestDto { Prompt = "" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetDietAdvice_WithAWhitespaceOnlyPrompt_ReturnsBadRequest()
    {
        var controller = new AiController(ConfigurationWithKey("test-key"));

        var result = await controller.GetDietAdvice(new AiController.AiRequestDto { Prompt = "    " });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetWorkoutAdvice_WithAnEmptyPrompt_ReturnsBadRequest()
    {
        var controller = new AiController(ConfigurationWithKey("test-key"));

        var result = await controller.GetWorkoutAdvice(new AiController.AiRequestDto { Prompt = "" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetWeeklyComparison_WithAnEmptyPrompt_ReturnsBadRequest()
    {
        var controller = new AiController(ConfigurationWithKey("test-key"));

        var result = await controller.GetWeeklyComparison(new AiController.AiRequestDto { Prompt = "" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetDietAdvice_WithNoApiKeyConfigured_ReturnsServerError()
    {
        var controller = new AiController(ConfigurationWithKey(null));

        var result = await controller.GetDietAdvice(new AiController.AiRequestDto { Prompt = "What should I eat?" });

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    [Fact]
    public async Task GetWorkoutAdvice_WithNoApiKeyConfigured_ReturnsServerError()
    {
        var controller = new AiController(ConfigurationWithKey(null));

        var result = await controller.GetWorkoutAdvice(new AiController.AiRequestDto { Prompt = "How is my training?" });

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }

    [Fact]
    public async Task GetWeeklyComparison_WithNoApiKeyConfigured_ReturnsServerError()
    {
        var controller = new AiController(ConfigurationWithKey(null));

        var result = await controller.GetWeeklyComparison(new AiController.AiRequestDto { Prompt = "Compare my weeks." });

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }
}
