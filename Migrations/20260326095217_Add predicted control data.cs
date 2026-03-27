using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class Addpredictedcontroldata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PredictedControlData",
                columns: table => new
                {
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Capacity = table.Column<string>(type: "TEXT", nullable: true),
                    Charge = table.Column<string>(type: "TEXT", nullable: true),
                    PriceSum = table.Column<float>(type: "REAL", nullable: true),
                    ChargeTimes = table.Column<string>(type: "TEXT", nullable: true),
                    DischargeTimes = table.Column<string>(type: "TEXT", nullable: true),
                    ChargeToCapacities = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictedControlData", x => x.TimeStamp);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PredictedControlData");
        }
    }
}
