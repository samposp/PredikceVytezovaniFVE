using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.SignalR;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Hubs;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using PredikceVytěžováníFVE.Models.Forecast;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Pages
{
    public class FVEModel(MqttDataService mqttData) : PageModel
    {
        public List<int> Battery = [];
        public List<DateTime> timestamps = [];
        public List<float> Consumption = [];
        public List<float> Production = [];
        public List<float> Grid = [];
        public List<float> ToBat = [];
        public List<float> FromBat = [];
        public List<float> Sell = [];
        public List<float> Buy = [];


        [BindProperty]
        public DateTime DataDate { get; set; }

        public void OnGet()
        {
            DataDate = DateTime.Now.AddDays(-1);

            GetData();
        }

        public void GetData()
        {
            var chartData = mqttData.GetMqttDataByDate(DataDate);

            chartData = DataFilterHelper.FilterDateTime(chartData);

            timestamps = chartData.Select(x => ConverterHelper.ToDateTime(x.Date, x.Time)).ToList();
            Battery = chartData.Where(x => x.SoC != null).Select(x => (int)x.SoC!).ToList();
            Production = chartData.Where(x => x.P_PV != null).Select(x => (float)x.P_PV!).ToList();
            Consumption = chartData.Where(x => x.P_HOME != null).Select(x => (float)x.P_HOME!).ToList();
            Grid = chartData.Where(x => x.P_GRID != null).Select(x => (float)-x.P_GRID!).ToList();
            ToBat = chartData.Where(x => x.ToBAT != null).Select(x => (float)-x.ToBAT!).ToList();
            FromBat = chartData.Where(x => x.FromBAT != null).Select(x => (float)-x.FromBAT!).ToList();
            Sell = chartData.Where(x => x.SELL != null).Select(x => (float)x.SELL!).ToList();
            Buy = chartData.Where(x => x.BUY != null).Select(x => (float)x.BUY!).ToList();

        } 

        public void OnPost()
        {
            GetData();
        }

    }
}
