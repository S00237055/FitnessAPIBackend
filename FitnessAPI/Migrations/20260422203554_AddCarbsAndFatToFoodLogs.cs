using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCarbsAndFatToFoodLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "carbsGrams",
                table: "FoodLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "fatGrams",
                table: "FoodLogs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "carbsGrams",
                table: "FoodLogs");

            migrationBuilder.DropColumn(
                name: "fatGrams",
                table: "FoodLogs");
        }
    }
}
