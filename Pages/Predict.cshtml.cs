using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Models.OpenWeather;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Pages {
    public class PredictModel : PageModel
    {
        private readonly PredicitonService _prediction;
        private readonly ILogger<PredictModel> _logger;
        private readonly FVEDbContext _database;

        public List<string> labels { get; set; } = new();
        public List<int> data { get; set; } = new();


        public PredictModel(ILogger<PredictModel> logger, FVEDbContext database) {
            _logger = logger;
            _database = database;
            _prediction = new(database);
        }

        public async Task OnGet() {
            for (int i = 0; i < 24; i++) {
                labels.Add(i.ToString());
            }
            await _prediction.Hourly();
            //data = prediction.HourlyPrice;

            DateTime today = DateTime.Now.AddDays(-1);

            var forecast = _database.forecastData.Where(x => x.TimeStamp == today).OrderBy(x=>x.TimeStamp);
            labels = forecast.Select(x=>x.TimeStamp.ToShortTimeString()).ToList();

            data = new();
            int previous = 0;
            forecast.Select(x => x.Value).ToList().ForEach(item => {
                data.Add(item - previous);
                previous = item;
            });
        }
    }
}
