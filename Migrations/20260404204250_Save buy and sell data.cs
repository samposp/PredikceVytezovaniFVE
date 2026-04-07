using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class Savebuyandselldata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GridBuy",
                table: "PredictedControlData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GridSell",
                table: "PredictedControlData",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GridBuy",
                table: "PredictedControlData");

            migrationBuilder.DropColumn(
                name: "GridSell",
                table: "PredictedControlData");
        }
    }
}
