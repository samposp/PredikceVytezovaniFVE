using NJsonSchema;

namespace PredikceVytěžováníFVE.Models {
    public class FVEData {
        public int BatteryPercentage;
        public int BatteryOutput;
        public int PVOutput;
        public double PVEnergyCumulative;
        public double OutputCumulative;
        public double InputCumulative;
        public DateTime Timestamp;
    }
}
