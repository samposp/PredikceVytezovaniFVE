using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PredikceVytěžováníFVE.Models.MeteoSource;
using PredikceVytěžováníFVE.Services;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Web;

namespace PredikceVytěžováníFVE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPredictionController : ControllerBase
    {
        readonly private MeteoSourceService meteoService = new();
        readonly private OpenWeatherService weatherService = new();


        //[HttpGet]
        //public async void GetWeather()
        //{
        //    PointQuery apiPoint = new()
        //    {
        //        placeID = "postal-cz-46601",
        //    };
        //    var data = await meteoService.GetPoint(apiPoint);
        //    Console.WriteLine(data.ToString());
        //}

        [HttpGet]
        public async void GetPrediction()
        {
            string lat = "50.7260878";
            string lon = "15.1675150";
            var data = await weatherService.Call(lat, lon);
            Console.WriteLine(data.ToString());
        }
    }
}
