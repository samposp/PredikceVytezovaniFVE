using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class changetables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mqttBos");

            migrationBuilder.CreateTable(
                name: "MqttData",
                columns: table => new
                {
                    Date = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<string>(type: "TEXT", nullable: false),
                    SoC = table.Column<int>(type: "INTEGER", nullable: false),
                    P_PV = table.Column<int>(type: "INTEGER", nullable: false),
                    P_Inv = table.Column<int>(type: "INTEGER", nullable: false),
                    P_OnGr = table.Column<int>(type: "INTEGER", nullable: false),
                    P_OffGr = table.Column<int>(type: "INTEGER", nullable: false),
                    P_Grid = table.Column<int>(type: "INTEGER", nullable: false),
                    P_Batt = table.Column<int>(type: "INTEGER", nullable: false),
                    PVenergy = table.Column<double>(type: "REAL", nullable: false),
                    Charge = table.Column<double>(type: "REAL", nullable: false),
                    Discharge = table.Column<double>(type: "REAL", nullable: false),
                    Feed = table.Column<double>(type: "REAL", nullable: false),
                    Consumption = table.Column<double>(type: "REAL", nullable: false),
                    Output = table.Column<double>(type: "REAL", nullable: false),
                    Input = table.Column<double>(type: "REAL", nullable: false),
                    Load = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mqttData", x => new { x.Date, x.Time });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MqttData");

            migrationBuilder.CreateTable(
                name: "mqttBos",
                columns: table => new
                {
                    DateTime = table.Column<string>(type: "TEXT", nullable: false),
                    Charge = table.Column<double>(type: "REAL", nullable: false),
                    Consumption = table.Column<double>(type: "REAL", nullable: false),
                    Discharge = table.Column<double>(type: "REAL", nullable: false),
                    Feed = table.Column<double>(type: "REAL", nullable: false),
                    Input = table.Column<double>(type: "REAL", nullable: false),
                    Load = table.Column<double>(type: "REAL", nullable: false),
                    Output = table.Column<double>(type: "REAL", nullable: false),
                    PVenergy = table.Column<double>(type: "REAL", nullable: false),
                    P_Batt = table.Column<int>(type: "INTEGER", nullable: false),
                    P_Grid = table.Column<int>(type: "INTEGER", nullable: false),
                    P_Inv = table.Column<int>(type: "INTEGER", nullable: false),
                    P_OffGr = table.Column<int>(type: "INTEGER", nullable: false),
                    P_OnGr = table.Column<int>(type: "INTEGER", nullable: false),
                    P_PV = table.Column<int>(type: "INTEGER", nullable: false),
                    SoC = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mqttBos", x => x.DateTime);
                });
        }
    }
}
