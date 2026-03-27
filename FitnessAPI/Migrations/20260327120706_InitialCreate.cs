using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BodyPart = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Equipment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GifUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exercise__A074AD0F3ECC3F91", x => x.ExerciseID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentWeight = table.Column<double>(type: "float", nullable: true),
                    GoalType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCAC4A771EA2", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "FoodLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    FoodName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Calories = table.Column<int>(type: "int", nullable: false),
                    ProteinGrams = table.Column<double>(type: "float", nullable: false),
                    DateEaten = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodLogs__5E5499A8A4C23B3E", x => x.LogID);
                    table.ForeignKey(
                        name: "FK__FoodLogs__UserID__440B1D61",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Workouts",
                columns: table => new
                {
                    WorkoutID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Workouts__E1C42A211B076B7C", x => x.WorkoutID);
                    table.ForeignKey(
                        name: "FK__Workouts__UserID__3C69FB99",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSets",
                columns: table => new
                {
                    SetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutID = table.Column<int>(type: "int", nullable: false),
                    ExerciseID = table.Column<int>(type: "int", nullable: false),
                    SetNumber = table.Column<int>(type: "int", nullable: false),
                    WeightKG = table.Column<double>(type: "float", nullable: false),
                    Reps = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WorkoutS__7E08473DB9C6D292", x => x.SetID);
                    table.ForeignKey(
                        name: "FK__WorkoutSe__Exerc__403A8C7D",
                        column: x => x.ExerciseID,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseID");
                    table.ForeignKey(
                        name: "FK__WorkoutSe__Worko__3F466844",
                        column: x => x.WorkoutID,
                        principalTable: "Workouts",
                        principalColumn: "WorkoutID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodLogs_UserID",
                table: "FoodLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserID",
                table: "Workouts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSets_ExerciseID",
                table: "WorkoutSets",
                column: "ExerciseID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSets_WorkoutID",
                table: "WorkoutSets",
                column: "WorkoutID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodLogs");

            migrationBuilder.DropTable(
                name: "WorkoutSets");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Workouts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
