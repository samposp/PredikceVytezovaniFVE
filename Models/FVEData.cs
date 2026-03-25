using NJsonSchema;

namespace PredikceVytěžováníFVE.Models {
    public class FVEData {
        public int? BatteryPercentage { get; set; }
        public int? BatteryOutput { get; set; }
        public int? PVOutput { get; set; }
        public float? Grid { get; set; }
        public float? Consumption { get; set; }
        public double? PVEnergyCumulative { get; set; }
        public double? OutputCumulative { get; set; }
        public double? InputCumulative { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
