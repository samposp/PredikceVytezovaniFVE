using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MQTTnet;
using PredikceVytìžováníFVE.Data;
using PredikceVytìžováníFVE.Helpers;
using PredikceVytìžováníFVE.Models;
using PredikceVytìžováníFVE.Models.DB;


namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly FVEDbContext _db;

        public FVEData initData = new(); 

        public IndexModel(ILogger<IndexModel> logger, FVEDbContext database)
        {
            _db = database;
            _logger = logger;
        }

        public void OnGet() {
            DateTime date = DateTime.Now;

            List<MqttDataBto> chartData = _db.mqttData.Where(x => x.TimeStamp.Date == date.Date).OrderBy(x => x.TimeStamp).ToList();
            if (chartData.Count != 0) {
                initData = ConverHelper.ToFVEData(chartData.Last());
            }
            
        }
    }
}
