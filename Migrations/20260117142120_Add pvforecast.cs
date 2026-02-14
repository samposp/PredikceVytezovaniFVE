using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class Addpvforecast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "MqttData",
                type: "TEXT",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MqttData");

            migrationBuilder.DropTable(
                name: "TimeValuePair");
        }
    }
}
