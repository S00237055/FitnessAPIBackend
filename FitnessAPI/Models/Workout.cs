using System;
using System.Collections.Generic;

namespace FitnessAPI.Models;

public partial class Workout
{
    public int WorkoutId { get; set; }

    public int UserId { get; set; }

    public DateTime? Date { get; set; }

    public string? Notes { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WorkoutSet> WorkoutSets { get; set; } = new List<WorkoutSet>();
}
