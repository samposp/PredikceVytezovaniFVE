using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.Forecast;
using PredikceVytěžováníFVE.Services;
using System.Reflection.Emit;
using WebApplication1.Pages;

namespace PredikceVytěžováníFVE.Pages
{
    public class PredictModel : PageModel
    {
        //private readonly PredicitonService prediction = new();
        private readonly ILogger<PredictModel> _logger;

        public List<string> labels { get; set; } = new();
        public List<double> data { get; set; } = new();


        public PredictModel(ILogger<PredictModel> logger) {
            _logger = logger;
        }

        public async Task OnGet() {
            for (int i = 0; i < 24; i++) {
                labels.Add(i.ToString());
            }
            //await prediction.Hourly();
            //data = prediction.HourlyPrice;

            //string latitude = "50.79";
            //string longitude = "15.145";


        }
    }
}
