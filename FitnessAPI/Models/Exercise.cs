using System;
using System.Collections.Generic;

namespace FitnessAPI.Models;

public partial class Exercise
{
    public int ExerciseId { get; set; }

    public string Name { get; set; } = null!;

    public string? BodyPart { get; set; }

    public string? Category { get; set; }

    public virtual ICollection<WorkoutSet> WorkoutSets { get; set; } = new List<WorkoutSet>();
}
