using System;
using System.Collections.Generic;

namespace FitnessAPI.Models;

public partial class WorkoutSet
{
    public int SetId { get; set; }

    public int WorkoutId { get; set; }

    public int ExerciseId { get; set; }

    public int SetNumber { get; set; }

    public double WeightKg { get; set; }

    public int Reps { get; set; }

    public virtual Exercise? Exercise { get; set; } = null!;

    public virtual Workout? Workout { get; set; } = null!;
}
