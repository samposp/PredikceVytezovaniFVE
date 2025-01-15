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

        [HttpGet]
        public async void GetPrediction()
        {
            PointQuery apiPoint = new()
            {
                placeID = "postal-cz-46601",
            };
            var data = await meteoService.GetPoint(apiPoint);
            Console.WriteLine(data.ToString());
        }
    }
}
