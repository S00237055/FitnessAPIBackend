using System;
using System.Collections.Generic;

namespace FitnessAPI.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public double? CurrentWeight { get; set; }

    public string? GoalType { get; set; }

    public virtual ICollection<FoodLog> FoodLogs { get; set; } = new List<FoodLog>();

    public virtual ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}
