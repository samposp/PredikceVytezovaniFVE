namespace PredikceVytěžováníFVE.Models {
    public class MqttResponse {
            public int SoC { get; set; }
            public int P_PV1 { get; set; }
            public int P_PV2 { get; set; }
            public int P_Inv { get; set; }
            public double F_Grid { get; set; }
            public int P_Eps { get; set; }
            public int F_Eps { get; set; }
            public int P_Meter { get; set; }
            public int P_Load { get; set; }
            public int P_Battery { get; set; }
            public double PVenergy { get; set; }
            public double Charge { get; set; }
            public double Discharge { get; set; }
            public double Feed { get; set; }
            public double Consumption { get; set; }
            public double Output { get; set; }
            public double Input { get; set; }
            public double Load { get; set; }
            public string ActDate { get; set; }
            public string ActTime { get; set; }

    }
}
