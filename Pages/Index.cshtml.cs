using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MQTTnet;
using PredikceVytìžováníFVE.Data;
using PredikceVytìžováníFVE.Helpers;
using PredikceVytìžováníFVE.Models;


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

        public void OnGet()
        {
            string date = DateTime.Now.ToString("d.M.yyyy");
            List<MqttData> chartData = _db.mqttData.Where(x => x.Date.Equals(date)).OrderBy(x => x.Time).ToList();
            initData = ConverHelper.ToFVEData(chartData.Last());
        }
    }
}
