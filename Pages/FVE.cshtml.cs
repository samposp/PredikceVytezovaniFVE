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
    public class FVEModel(MqttDataService mqttData, SpotSoapService spotService) : PageModel
    {
        public List<int> Battery { get; set; } = [];
        public List<DateTime> timestamps { get; set; } = [];
        public List<float> Consumption { get; set; } = [];
        public List<float> Production { get; set; } = [];
        public List<float> Grid { get; set; } = [];
        public List<float> ToBat { get; set; } = [];
        public List<float> FromBat { get; set; } = [];
        public List<float> Sell { get; set; } = [];
        public List<float> Buy { get; set; } = [];
        public List<float> Cost { get; set; } = [];
        public List<DateTime> CosttimeStamp { get; set; } = [];

        public bool CheckBuyData = false;


        [BindProperty(SupportsGet = true)]
        public DateTime DataDate { get; set; } = DateTime.Now.AddDays(-1);

        public async Task OnGet()
        {
            await GetData();
        }

        public async Task GetData()
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

            if (!CheckBuyData)
                return;
           var spotData = await spotService.GetSoapData(DataDate);
            DateTime last = DataDate.Date;
            float lastVal = 0;
            foreach (var data in chartData)
            {
                var hours = data.DateTime?.Subtract(last).TotalMinutes / 60;
                var price = spotData.Where(x => x.DateTime <= data.DateTime).Select(x => x.Value).Max();
                lastVal += (float)((float)price * (-data.P_GRID / 1000) * hours);
                Cost.Add(lastVal);
                CosttimeStamp.Add(data.DateTime ?? DataDate);
                last = data.DateTime ?? last;
            }

        } 

        public IActionResult OnPost()
        {
            return RedirectToPage(new { DataDate = DataDate.ToString("yyyy-MM-dd") });
        }

    }
}
