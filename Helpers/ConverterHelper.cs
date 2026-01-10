using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using PublicOTEService;

namespace PredikceVytěžováníFVE.Helpers {
    public class ConverterHelper {
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
        public static TimeValuePair ToTimeValuePair(SpotBo spotBo) {
            return new TimeValuePair(spotBo.DateTime, spotBo.Value);
        }
        public static SpotBo ToSpotBo(GetDamPricePeriodEResponseItem spotPrice) {
            DateTime dateTime = spotPrice.Date;
            dateTime = dateTime.AddHours(spotPrice.PeriodIndex - 1);
            return new SpotBo {
                DateTime = dateTime,
                Value = (decimal)spotPrice.Price
            };
        }
    }
}
