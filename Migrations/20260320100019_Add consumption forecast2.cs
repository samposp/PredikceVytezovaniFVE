using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class Addconsumptionforecast2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HourlyConsumptionData",
                table: "HourlyConsumptionData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsumptionData",
                table: "ConsumptionData");

            migrationBuilder.RenameTable(
                name: "HourlyConsumptionData",
                newName: "HourlyData");

            migrationBuilder.RenameTable(
                name: "ConsumptionData",
                newName: "ConsumptionForecastData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HourlyData",
                table: "HourlyData",
                column: "TimeStamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsumptionForecastData",
                table: "ConsumptionForecastData",
                column: "index");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HourlyData",
                table: "HourlyData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsumptionForecastData",
                table: "ConsumptionForecastData");

            migrationBuilder.RenameTable(
                name: "HourlyData",
                newName: "HourlyConsumptionData");

            migrationBuilder.RenameTable(
                name: "ConsumptionForecastData",
                newName: "ConsumptionData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HourlyConsumptionData",
                table: "HourlyConsumptionData",
                column: "TimeStamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsumptionData",
                table: "ConsumptionData",
                column: "index");
        }
    }
}
