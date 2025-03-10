using PredikceVytěžováníFVE.Models;

namespace PredikceVytěžováníFVE.Helpers {
    public class ConverHelper {
        public static FVEData ToFVEData(MqttData message) {
            return new() {
                BatteryPercentage = message.SoC,
                BatteryOutput = message.P_Batt,
                PVOutput = message.P_Inv,
                PVEnergyCumulative = message.PVenergy,
                OutputCumulative = message.Output,
                InputCumulative = message.Consumption,
                Timestamp = DateTime.Parse(message.Date + " " + message.Time)
            };
        }
    }
}
