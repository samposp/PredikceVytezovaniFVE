using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PredikceVytěžováníFVE.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mqttBos",
                columns: table => new
                {
                    DateTime = table.Column<string>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_mqttBos", x => x.DateTime);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mqttBos");
        }
    }
}
