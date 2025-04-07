using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class add_timestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_spotData",
                table: "spotData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mqttData",
                table: "mqttData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_forecastData",
                table: "forecastData");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "spotData");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "P_Batt",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "P_Inv",
                table: "mqttData");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "forecastData");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "spotData",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "P_Grid",
                table: "mqttData",
                newName: "P_GRID");

            migrationBuilder.RenameColumn(
                name: "P_OnGr",
                table: "mqttData",
                newName: "P_EPS");

            migrationBuilder.RenameColumn(
                name: "P_OffGr",
                table: "mqttData",
                newName: "P_BAT");

            migrationBuilder.RenameColumn(
                name: "Output",
                table: "mqttData",
                newName: "ToBAT");

            migrationBuilder.RenameColumn(
                name: "Load",
                table: "mqttData",
                newName: "SELL");

            migrationBuilder.RenameColumn(
                name: "Input",
                table: "mqttData",
                newName: "PVForecast");

            migrationBuilder.RenameColumn(
                name: "Feed",
                table: "mqttData",
                newName: "FromBAT");

            migrationBuilder.RenameColumn(
                name: "Discharge",
                table: "mqttData",
                newName: "Consumed");

            migrationBuilder.RenameColumn(
                name: "Consumption",
                table: "mqttData",
                newName: "BUY_T");

            migrationBuilder.RenameColumn(
                name: "Charge",
                table: "mqttData",
                newName: "BUY");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "mqttData",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "forecastData",
                newName: "TimeStamp");

            migrationBuilder.AlterColumn<double>(
                name: "ToBAT_T",
                table: "mqttData",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_spotData",
                table: "spotData",
                column: "TimeStamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mqttData",
                table: "mqttData",
                column: "TimeStamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_forecastData",
                table: "forecastData",
                column: "TimeStamp");

            migrationBuilder.CreateTable(
                name: "testData",
                columns: table => new
                {
                    dateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testData", x => x.dateTime);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "testData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_spotData",
                table: "spotData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mqttData",
                table: "mqttData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_forecastData",
                table: "forecastData");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "spotData",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "P_GRID",
                table: "mqttData",
                newName: "P_Grid");

            migrationBuilder.RenameColumn(
                name: "ToBAT",
                table: "mqttData",
                newName: "Output");

            migrationBuilder.RenameColumn(
                name: "SELL",
                table: "mqttData",
                newName: "Load");

            migrationBuilder.RenameColumn(
                name: "P_EPS",
                table: "mqttData",
                newName: "P_OnGr");

            migrationBuilder.RenameColumn(
                name: "P_BAT",
                table: "mqttData",
                newName: "P_OffGr");

            migrationBuilder.RenameColumn(
                name: "PVForecast",
                table: "mqttData",
                newName: "Input");

            migrationBuilder.RenameColumn(
                name: "FromBAT",
                table: "mqttData",
                newName: "Feed");

            migrationBuilder.RenameColumn(
                name: "Consumed",
                table: "mqttData",
                newName: "Discharge");

            migrationBuilder.RenameColumn(
                name: "BUY_T",
                table: "mqttData",
                newName: "Consumption");

            migrationBuilder.RenameColumn(
                name: "BUY",
                table: "mqttData",
                newName: "Charge");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "mqttData",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "forecastData",
                newName: "Time");

            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "spotData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ToBAT_T",
                table: "mqttData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "mqttData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "P_Batt",
                table: "mqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "P_Inv",
                table: "mqttData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "forecastData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_spotData",
                table: "spotData",
                columns: new[] { "Date", "Time" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_mqttData",
                table: "mqttData",
                columns: new[] { "Date", "Time" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_forecastData",
                table: "forecastData",
                columns: new[] { "Date", "Time" });
        }
    }
}
