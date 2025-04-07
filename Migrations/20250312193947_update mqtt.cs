using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class updatemqtt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Consumed_T",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FromBAT_T",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PRICE_CZK",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PVenergy_T",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "P_HOME",
                table: "mqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SELL_T",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Temp24h",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Temp48h",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ToBAT_T",
                table: "mqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "consPredict",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Consumed_T",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "FromBAT_T",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "PRICE_CZK",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "PVenergy_T",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "P_HOME",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "SELL_T",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "Temp24h",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "Temp48h",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "ToBAT_T",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "consPredict",
                table: "mqttData");
        }
    }
}
