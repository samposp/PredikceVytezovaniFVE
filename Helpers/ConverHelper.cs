using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using PredikceVytěžováníFVE.Models.Forecast;

namespace PredikceVytěžováníFVE.Helpers
{
    public class ConverHelper {
        public static FVEData ToFVEData(MqttDataBto message) {
            return new() {
                BatteryPercentage = message.SoC,
                BatteryOutput = message.P_BAT,
                PVOutput = message.P_PV,
                PVEnergyCumulative = message.PVenergy,
                OutputCumulative = message.P_GRID,
                InputCumulative = message.P_HOME,
                Timestamp = message.TimeStamp
            };
        }
        public static FVEData ToFVEData(MqttData message) {
            return new() {
                BatteryPercentage = message.SoC,
                BatteryOutput = message.P_BAT,
                PVOutput = message.P_PV,
                PVEnergyCumulative = message.PVenergy,
                OutputCumulative = message.P_GRID,
                InputCumulative = message.P_HOME,
                Timestamp = DateTime.Parse(message.Date + " " + message.Time)
            };
        }
        public static MqttDataBto ToMqttDataBto(MqttData data) => new() {
            BUY = data.BUY,
            BUY_T = data.BUY_T,
            P_BAT = data.P_BAT,
            P_PV = data.P_PV,
            PVenergy = data.PVenergy,
            P_GRID = data.P_GRID,
            P_HOME = data.P_HOME,
            SoC = data.SoC,
            Consumed_T = data.Consumed_T,
            PRICE_CZK = data.PRICE_CZK,
            SELL = data.SELL,
            SELL_T = data.SELL_T,
            P_EPS = data.P_EPS,
            PVForecast = data.PVForecast,
            PVenergy_T = data.PVenergy_T,
            ToBAT = data.ToBAT,
            ToBAT_T = data.ToBAT_T,
            FromBAT = data.FromBAT,
            FromBAT_T = data.FromBAT_T,
            Consumed = data.Consumed,
            Temp24h = data.Temp24h,
            Temp48h = data.Temp48h,
            TimeStamp = DateTime.Parse(data.Date + " " + data.Time)
        };
    }
}
