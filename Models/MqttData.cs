using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models {
    [PrimaryKey(nameof(Date), nameof(Time))]
    public class MqttData {
            public int SoC { get; set; }        // procento nabití baterie
            public int P_PV { get; set; }       // výkon stringu
            public int P_Inv { get; set; }      // výkon měniče
            public int P_OnGr { get; set; }     //
            public int P_OffGr { get; set; }    //
            public int P_Grid { get; set; }     //
            public int P_Batt { get; set; }     // z baterie do domu nebo její nabíjení podle znaménka
            public double PVenergy { get; set; }    //
            public double Charge { get; set; }      // množství energie do baterie od půlnoci
            public double Discharge { get; set; }   // množství energie z baterie od půlnoci
            public double Feed { get; set; }        // množství energie ze sítě od půlnoci
            public double Consumption { get; set; } // spotřeba domu od půlnoci
            public double Output { get; set; }      // přetoky do sítě od půlnoci
            public double Input { get; set; }
            public double Load { get; set; }
            public string Date { get; set; }        // časová značka
            public string Time { get; set; }
    }
}
