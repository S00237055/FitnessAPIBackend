using System;
using System.Collections.Generic;

namespace FitnessAPI.Models;

public partial class FoodLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string FoodName { get; set; } = null!;

    public int Calories { get; set; }

    public double ProteinGrams { get; set; }

    public DateTime? DateEaten { get; set; }

    public virtual User User { get; set; } = null!;
}
