namespace FitnessAPI.Models
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public double? CurrentWeight { get; set; }
        public string? GoalType { get; set; }
        public string Token { get; set; } = null!;
    }
}
