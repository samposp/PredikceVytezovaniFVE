using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MQTTnet;
using Newtonsoft.Json.Linq;
using PredikceVytìžováníFVE.Data;
using PredikceVytìžováníFVE.Helpers;
using PredikceVytìžováníFVE.Models;
using PredikceVytìžováníFVE.Services;


namespace PredikceVytìžováníFVE.Pages
{
    public class IndexModel(ILogger<IndexModel> logger, MqttDataService mqttData) : PageModel
    {

        public MqttData initData = new();
        public List<int> Battery = [];
        public List<DateTime> timestamps = [];
        public List<float> Consumption = [];
        public List<float> Production = [];
        public List<float> Grid = [];
        public DateTime LastUpdate;
        //public List<float> ToBat = [];
        //public List<float> FromBat = [];
        //public List<float> Sell = [];
        //public List<float> Buy = [];


        [BindProperty]
        public DateTime DataDate { get; set; } = DateTime.Now;

        public void OnGet()
        {
            GetData();
        }

        public void GetData()
        {
            var chartData = mqttData.GetMqttDataByDate(DataDate);

            chartData = DataFilterHelper.FilterDateTime(chartData, new TimeSpan(0,15,0));

            if (chartData.Count != 0)
            {
                initData = chartData.Last();
                LastUpdate = ConverterHelper.ToDateTime(initData.Date, initData.Time);
            }
            timestamps = chartData.Select(x => ConverterHelper.ToDateTime(x.Date, x.Time)).ToList();
            Battery = chartData.Where(x => x.SoC != null).Select(x => (int)x.SoC!).ToList();
            Production = chartData.Where(x => x.P_PV != null).Select(x => (float)x.P_PV!).ToList();
            Consumption = chartData.Where(x => x.P_HOME != null).Select(x => (float)x.P_HOME!).ToList();
            Grid = chartData.Where(x => x.P_GRID != null).Select(x => (float)-x.P_GRID!).ToList();
            //ToBat = chartData.Where(x => x.ToBAT != null).Select(x => (float)-x.ToBAT!).ToList();
            //FromBat = chartData.Where(x => x.FromBAT != null).Select(x => (float)-x.FromBAT!).ToList();
            //Sell = chartData.Where(x => x.SELL != null).Select(x => (float)x.SELL!).ToList();
            //Buy = chartData.Where(x => x.BUY != null).Select(x => (float)x.BUY!).ToList();

        } 

    }
}
