using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixedCasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fatGrams",
                table: "FoodLogs",
                newName: "FatGrams");

            migrationBuilder.RenameColumn(
                name: "carbsGrams",
                table: "FoodLogs",
                newName: "CarbsGrams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FatGrams",
                table: "FoodLogs",
                newName: "fatGrams");

            migrationBuilder.RenameColumn(
                name: "CarbsGrams",
                table: "FoodLogs",
                newName: "carbsGrams");
        }
    }
}
