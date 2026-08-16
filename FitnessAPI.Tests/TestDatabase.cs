using FitnessAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessAPI.Tests;

/// <summary>
/// Creates an isolated in-memory database for a single test.
/// Each call uses a fresh database name so that tests cannot affect one another
/// and can safely run in parallel.
/// </summary>
public static class TestDatabase
{
    public static FitnessAppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<FitnessAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FitnessAppDbContext(options);
    }

    /// <summary>Adds a user with a correctly salted and hashed password.</summary>
    public static User SeedUser(
        FitnessAppDbContext context,
        string username = "testuser",
        string password = "Password123",
        double? weight = 80,
        string? goal = "Build Muscle")
    {
        var salt = PasswordHelper.CreateSalt();
        var hash = PasswordHelper.HashPassword(password, salt);

        var user = new User
        {
            Username = username,
            PasswordHash = Convert.ToBase64String(hash),
            PasswordSalt = Convert.ToBase64String(salt),
            CurrentWeight = weight,
            GoalType = goal
        };

        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    public static Exercise SeedExercise(
        FitnessAppDbContext context,
        string name = "Bench Press",
        string bodyPart = "chest")
    {
        var exercise = new Exercise
        {
            Name = name,
            BodyPart = bodyPart,
            Target = "pectorals",
            Equipment = "barbell",
            Instructions = "Lie on the bench and press the bar upward."
        };

        context.Exercises.Add(exercise);
        context.SaveChanges();
        return exercise;
    }

    public static FoodLog SeedFoodLog(
        FitnessAppDbContext context,
        int userId,
        string foodName = "Chicken Breast",
        int calories = 165,
        DateTime? eatenAt = null)
    {
        var log = new FoodLog
        {
            UserId = userId,
            FoodName = foodName,
            Calories = calories,
            ProteinGrams = 31,
            CarbsGrams = 0,
            FatGrams = 3.6,
            DateEaten = eatenAt ?? DateTime.Now
        };

        context.FoodLogs.Add(log);
        context.SaveChanges();
        return log;
    }
}
