using Microsoft.AspNetCore.Mvc;
using PredikceVytěžováníFVE.Services;
using PublicOTEService;

namespace PredikceVytěžováníFVE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPredictionController : ControllerBase
    {
        //readonly private PVForcastService pvForcastService = new();
        
        //readonly private ForecastService forecastService = new();

        //readonly private MQTTService mqttService = new("147.230.76.38");

        [HttpGet]
        public async void GetPrediction()
        {   
            string latitude = "50.79";
            string longitude = "15.145";
            string azimuth = "-15";
            string peakPower = "19.9";
            string declination = "35";
            //var a = await forecastService.GetWatthours(latitude, longitude, peakPower, declination, azimuth);
            //Console.WriteLine(a);
        }

        [HttpGet("/Spot")]
        public async void GetSpot() {
            PublicDataServiceSoapClient client = new();
            DateTime tomorrow = DateTime.Now.AddDays(1);
            GetDamPriceEResponse damPrice = await client.GetDamPriceEAsync(tomorrow, tomorrow, 1, 24, false);
            Console.WriteLine(damPrice.Result.Length);
        }

        [HttpGet("/MQTT")]
        public async void GetMqtt() {
            //await mqttService.Connect();
            //await mqttService.Subscribe("FVE/Ibehej_TX");
        }
    }
}
