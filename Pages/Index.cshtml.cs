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

        public FVEData initData = new();
        public List<int> Battery = [];
        public List<DateTime> timestamps = [];

        [BindProperty]
        public DateTime DataDate { get; set; }

        public void OnGet()
        {
            //DataDate = DateTime.Now.AddDays(-1);
            DataDate = new DateTime(2026, 1, 22);
            //var data = await spotService.GetSoapData(DataDate);
            GetData();

        }

        public void GetData()
        {
            var chartData = mqttData.GetMqttDataByDate(DataDate);

            chartData = DataFilterHelper.FilterDateTime(chartData);

            if (chartData.Count != 0)
                initData = ConverterHelper.ToFVEData(chartData.Last());
            timestamps = chartData.Select(x => ConverterHelper.ToDateTime(x.Date, x.Time)).ToList();
            //Battery = chartData.Where(x => x.SoC != null).Select(x => (int)x.SoC!).ToList();
            Battery = chartData.Where(x => x.P_PV != null).Select(x => (int)x.P_PV!).ToList();


        } 

        public void OnPost()
        {
            GetData();
        }

    }
}
