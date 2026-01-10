using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class Mqttdatachanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "P_Grid",
                table: "MqttData",
                newName: "P_GRID");

            migrationBuilder.AddColumn<double>(
                name: "BUY",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BUY_T",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FromBAT",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FromBAT_T",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "GRID_LIMIT",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "PRICE_CZK",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PVenergy_T",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "P_BAT",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "P_EPS",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "P_GRID_L1",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_GRID_L2",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_GRID_L3",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_HOME",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_HOME_L1",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_HOME_L2",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_HOME_L3",
                table: "MqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SELL",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SELL_T",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ToBAT",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ToBAT_T",
                table: "MqttData",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BUY",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "BUY_T",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "FromBAT",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "FromBAT_T",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "GRID_LIMIT",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "PRICE_CZK",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "PVenergy_T",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_BAT",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_EPS",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_GRID_L1",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_GRID_L2",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_GRID_L3",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_HOME",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_HOME_L1",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_HOME_L2",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "P_HOME_L3",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "SELL",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "SELL_T",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "ToBAT",
                table: "MqttData");

            migrationBuilder.DropColumn(
                name: "ToBAT_T",
                table: "MqttData");

            migrationBuilder.RenameColumn(
                name: "P_GRID",
                table: "MqttData",
                newName: "P_Grid");
        }
    }
}
