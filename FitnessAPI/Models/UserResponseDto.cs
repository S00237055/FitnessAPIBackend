namespace FitnessAPI.Models
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public double? CurrentWeight { get; set; }
        public string? GoalType { get; set; }
    }
}
