using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace PredikceVytěžováníFVE.Models.DB {

    public class MqttData {
        public int SoC { get; set; }
        public int P_PV { get; set; }
        public int P_HOME { get; set; }
        public int P_EPS { get; set; }
        public int P_GRID { get; set; }
        public int P_BAT { get; set; }
        public double PRICE_CZK { get; set; }
        public double PVForecast { get; set; }
        public double PVenergy { get; set; }
        public double PVenergy_T { get; set; }
        public double ToBAT { get; set; }
        public double ToBAT_T { get; set; }
        public double FromBAT { get; set; }
        public double FromBAT_T { get; set; }
        public double SELL { get; set; }
        public double SELL_T { get; set; }
        public double BUY { get; set; }
        public double BUY_T { get; set; }
        public double Consumed { get; set; }
        public double Consumed_T { get; set; }
        public double Temp24h { get; set; }
        public double Temp48h { get; set; }
        public double consPredict { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }
    [PrimaryKey(nameof(TimeStamp))]
    public class MqttDataBto{
        public int SoC { get; set; }
        public int P_PV { get; set; }
        public int P_HOME { get; set; }
        public int P_EPS { get; set; }
        public int P_GRID { get; set; }
        public int P_BAT { get; set; }
        public double PRICE_CZK { get; set; }
        public double PVForecast { get; set; }
        public double PVenergy { get; set; }
        public double PVenergy_T { get; set; }
        public double ToBAT { get; set; }
        public double ToBAT_T { get; set; }
        public double FromBAT { get; set; }
        public double FromBAT_T { get; set; }
        public double SELL { get; set; }
        public double SELL_T { get; set; }
        public double BUY { get; set; }
        public double BUY_T { get; set; }
        public double Consumed { get; set; }
        public double Consumed_T { get; set; }
        public double Temp24h { get; set; }
        public double Temp48h { get; set; }
        public double consPredict { get; set; }
        public DateTime TimeStamp { get; set; }
    }


}
