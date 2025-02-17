using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytìžováníFVE.Models;
using PredikceVytìžováníFVE.Services;
using ServiceReference1;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        ForecastService forecastService = new ForecastService();
        PVForcastService pvForcastService = new PVForcastService();


        public List<string> labels { get; set; } = new();

        public List<decimal> data { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            //PublicDataServiceSoapClient client = new();
            //DateTime tomorrow = DateTime.Now;
            //GetDamPriceEResponse damPrice = await client.GetDamPriceEAsync(tomorrow, tomorrow, 1, 24, true);
            //labels = damPrice.Result.Select(x => x.Hour).ToList();
            //data = damPrice.Result.Select(x => x.Price).ToList();

            string latitude = "50.79";
            string longitude = "15.145";
            //IEnumerable<TimeValuePair> dataPair = await forecastService.GetWatthours(latitude, longitude, "10");
            IEnumerable<TimeValuePair> dataPair = await pvForcastService.GetPrediciton(latitude, longitude);

            labels = dataPair.Select(x => x.Time.ToShortTimeString()).ToList();
            data = dataPair.Select(x => x.Value).ToList();
        }
    }
}
