using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class changedataforecast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeValuePair");

            migrationBuilder.CreateTable(
                name: "ForecastSolarData",
                columns: table => new
                {
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastSolarData", x => x.TimeStamp);
                });

            migrationBuilder.CreateTable(
                name: "PvForecastData",
                columns: table => new
                {
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PvForecastData", x => x.TimeStamp);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastSolarData");

            migrationBuilder.DropTable(
                name: "PvForecastData");

            migrationBuilder.CreateTable(
                name: "TimeValuePair",
                columns: table => new
                {
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeValuePair", x => x.DateTime);
                });
        }
    }
}
