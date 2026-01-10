using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class forecastspot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forecastData",
                columns: table => new
                {
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forecastData", x => new { x.Date, x.Time });
                });

            migrationBuilder.CreateTable(
                name: "spotData",
                columns: table => new
                {
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spotData", x => new { x.Date, x.Time });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forecastData");

            migrationBuilder.DropTable(
                name: "spotData");
        }
    }
}
