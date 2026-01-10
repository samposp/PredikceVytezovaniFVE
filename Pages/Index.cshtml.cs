using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MQTTnet;
using Newtonsoft.Json.Linq;
using PredikceVytìžováníFVE.Data;
using PredikceVytìžováníFVE.Helpers;
using PredikceVytìžováníFVE.Models;
using PredikceVytìžováníFVE.Services;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;


namespace WebApplication1.Pages
{
    public class IndexModel(ILogger<IndexModel> logger, FVEDbContext db, SpotSoapService spotService) : PageModel
    {

        public FVEData initData = new();
        public List<string> timestamp = new() {"0:00", "1:00", "2:00", "3:00", "4:00", "5:00", "6:00", "7:00", "8:00", "9:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00" };
        public List<int> Battery = new() { 100, 95, 90, 85, 80, 75, 65, 55, 45, 35, 25, 20, 20, 20, 20, 20, 20, 50, 60, 70, 80, 85, 90, 95, 100 };

        public async Task OnGet()
        {
            var data = await spotService.GetSoapData(DateTime.Now.AddDays(1));
            foreach (var item in data)
            {
                logger.LogInformation("datetime: {date}, price: {price}czk", item.Time.ToShortTimeString(), item.Value);
            }
            string date = DateTime.Now.ToString("d.M.yyyy");
            date = "28.2.2025";
            List<MqttData> chartData = db.MqttData.Where(x => x.Date.Equals(date)).OrderBy(x => x.Time).ToList();
            //initData = ConverterHelper.ToFVEData(chartData.Last());

            initData = new() {
                BatteryPercentage = 95,
                PVOutput = 1248,
                BatteryOutput = 493
            };
        }
    }
}
