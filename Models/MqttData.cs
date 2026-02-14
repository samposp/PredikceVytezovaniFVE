using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Helpers;

namespace PredikceVytěžováníFVE.Models {
    [PrimaryKey(nameof(Date), nameof(Time))]
    public class MqttData : HasTimeStamp {
        public int? SoC { get; set; }        // procento nabití baterie
        public int? P_PV { get; set; }       // výkon stringu
        public int? P_Inv { get; set; }      // výkon měniče
        public int? P_OnGr { get; set; }     //
        public int? P_OffGr { get; set; }    //
        public int? P_Batt { get; set; }     // z baterie do domu nebo její nabíjení podle znaménka
        public double? PVenergy { get; set; }    //
        public double? Charge { get; set; }      // množství energie do baterie od půlnoci
        public double? Discharge { get; set; }   // množství energie z baterie od půlnoci
        public double? Feed { get; set; }        // množství energie ze sítě od půlnoci
        public double? Consumption { get; set; } // spotřeba domu od půlnoci
        public double? Output { get; set; }      // přetoky do sítě od půlnoci
        public double? Input { get; set; }
        public double? Load { get; set; }
        public string? Date { get; set; }        // časová značka
        public string? Time { get; set; }
        public DateTime? DateTime { get; set; }

        public int? GRID_LIMIT { get; set; }
        public double? PRICE_CZK { get; set; }
        public int? P_HOME { get; set; }
        public int? P_HOME_L1 { get; set; }
        public int? P_HOME_L2 { get; set; }
        public int? P_HOME_L3 { get; set; }
        public double? P_EPS { get; set; }
        public int? P_GRID { get; set; }
        public int? P_GRID_L1 { get; set; }
        public int? P_GRID_L2 { get; set; }
        public int? P_GRID_L3 { get; set; }
        public int? P_BAT { get; set; }
        public double? PVenergy_T { get; set; }
        public double? ToBAT { get; set; }
        public double? ToBAT_T { get; set; }
        public double? FromBAT { get; set; }
        public double? FromBAT_T { get; set; }
        public double? SELL { get; set; }
        public double? SELL_T { get; set; }
        public double? BUY { get; set; }
        public double? BUY_T { get; set; }


        public string? FullMessage { get; set; }

    }
}
