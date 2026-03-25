using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class Addconsumptionforecast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumptionData",
                columns: table => new
                {
                    index = table.Column<DateTime>(type: "TEXT", nullable: false),
                    forecast = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionData", x => x.index);
                });

            migrationBuilder.CreateTable(
                name: "HourlyConsumptionData",
                columns: table => new
                {
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    P_HOME = table.Column<decimal>(type: "TEXT", nullable: true),
                    Temperature = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyConsumptionData", x => x.TimeStamp);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumptionData");

            migrationBuilder.DropTable(
                name: "HourlyConsumptionData");
        }
    }
}
