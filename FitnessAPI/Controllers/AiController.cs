using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace FitnessAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AiController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient(); // Used to make requests to Google
        }

        [HttpPost("DietAdvice")]
        public async Task<IActionResult> GetDietAdvice([FromBody] AiRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest("Prompt cannot be empty.");

            // Gets the secret API key from appsettings.Development.json
            var apiKey = _configuration["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, "API Key is missing in configuration.");

            // Google Gemini API URL
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            // payload exactly how Google Gemini expects it
            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = request.Prompt } } }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                // Sends the request to Google's AI
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                // Parses the JSON response from Google to grab just the text
                using var jsonDoc = JsonDocument.Parse(responseString);
                var adviceText = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                // Sends the text back to React Native!
                return Ok(new { advice = adviceText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error communicating with AI: {ex.Message}");
            }
        }
    }

    // DTO to catch the incoming prompt from React Native
    public class AiRequestDto
    {
        public required string Prompt { get; set; }
    }
}