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

namespace PredikceVytěžováníFVE.Pages
{
    public class FVEModel : PageModel
    {
        private readonly FVEDbContext _db;

        public List<int> BatteryLevel = new();
        public List<string> Timestapms = new();
        public List<int> FVEPower = new();

        public FVEData initData = new();

        public FVEModel(FVEDbContext database)
        {
            _db = database;
        }
        public void OnGet()
        {
            GetData();
        }

        private void GetData() {
            string date = DateTime.Now.ToString("d.M.yyyy");
            date = "28.2.2025";
            List<MqttData> chartData = _db.MqttData.Where(x => x.Date.Equals(date)).OrderBy(x => x.Time).ToList();
            initData = ConverterHelper.ToFVEData(chartData.Last());
            BatteryLevel = chartData.Select(x => x.SoC).ToList();

            FVEPower = chartData.Select(x => x.P_PV).ToList();

            //Timestapms = new();
            //double previous = 0;
            //a.Select(x => (double)x.Value).ToList().ForEach(item => {
            //    data.Add((double)(item - previous));
            //    previous = item;
            //});

        } 
    }
}
