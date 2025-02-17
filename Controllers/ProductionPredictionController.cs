using Microsoft.AspNetCore.Mvc;
using PredikceVytěžováníFVE.Services;
using ServiceReference1;

namespace PredikceVytěžováníFVE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPredictionController : ControllerBase
    {
        readonly private PVForcastService pvForcastService = new();
        
        readonly private ForecastService forecastService = new();

        [HttpGet]
        public async void GetPrediction()
        {
            string latitude = "50.79";
            string longitude = "15.145";
            await forecastService.GetWatthoursDay(latitude, longitude, "10");
        }

        [HttpGet("/Spot")]
        public async void GetSpot() {
            PublicDataServiceSoapClient client = new();
            DateTime tomorrow = DateTime.Now.AddDays(1);
            GetDamPriceEResponse damPrice = await client.GetDamPriceEAsync(tomorrow, tomorrow, 1, 24, false);
            Console.WriteLine(damPrice.Result.Length);
        }
    }
}
