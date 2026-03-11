using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FitnessAPI.Models;

public partial class FitnessAppDbContext : DbContext
{
    public FitnessAppDbContext()
    {
    }

    public FitnessAppDbContext(DbContextOptions<FitnessAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<FoodLog> FoodLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workout> Workouts { get; set; }

    public virtual DbSet<WorkoutSet> WorkoutSets { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("PK__Exercise__A074AD0F3ECC3F91");

            entity.Property(e => e.ExerciseId).HasColumnName("ExerciseID");
            entity.Property(e => e.BodyPart).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<FoodLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__FoodLogs__5E5499A8A4C23B3E");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.DateEaten)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FoodName).HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.FoodLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodLogs__UserID__440B1D61");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC4A771EA2");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.GoalType).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.WorkoutId).HasName("PK__Workouts__E1C42A211B076B7C");

            entity.Property(e => e.WorkoutId).HasColumnName("WorkoutID");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Workouts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Workouts__UserID__3C69FB99");
        });

        modelBuilder.Entity<WorkoutSet>(entity =>
        {
            entity.HasKey(e => e.SetId).HasName("PK__WorkoutS__7E08473DB9C6D292");

            entity.Property(e => e.SetId).HasColumnName("SetID");
            entity.Property(e => e.ExerciseId).HasColumnName("ExerciseID");
            entity.Property(e => e.WeightKg).HasColumnName("WeightKG");
            entity.Property(e => e.WorkoutId).HasColumnName("WorkoutID");

            entity.HasOne(d => d.Exercise).WithMany(p => p.WorkoutSets)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkoutSe__Exerc__403A8C7D");

            entity.HasOne(d => d.Workout).WithMany(p => p.WorkoutSets)
                .HasForeignKey(d => d.WorkoutId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkoutSe__Worko__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
