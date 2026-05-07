using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Pages
{
    public class FVEModel(MqttDataService mqttData) : PageModel
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


        [BindProperty(SupportsGet = true)]
        public DateTime DataDate { get; set; } = DateTime.Now.AddDays(-1);

        public void OnGet()
        {
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

        public IActionResult OnPost()
        {
            return RedirectToPage(new { DataDate = DataDate.ToString("yyyy-MM-dd") });
        }

    }
}
