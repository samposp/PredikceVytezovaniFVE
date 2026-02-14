using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models.OpenWeather;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Pages; 
public class PredictModel(ILogger<PredictModel> logger, PVForecastService fveForecastService, SpotSoapService spot, PredictitonService predictionService, ForecastService forecastService) : PageModel
{
    [BindProperty]
    public DateTime DataDate { get; set; }

    [BindProperty]
    public List<DateTime> SpotTimeStamps { get; set; } = [];
    [BindProperty]
    public List<float> SpotPrice { get; set; } = [];

    [BindProperty]
    public List<DateTime> FVEPredictionTimeStamps { get; set; } = [];
    [BindProperty]
    public List<float> FVEPrediction { get; set; } = [];

    [BindProperty]
    public List<DateTime> ConsumptionPredictionTimeStamps { get; set; } = [];
    [BindProperty]
    public List<float> ConsumptionPrediction { get; set; } = [];

    [BindProperty]
    public bool UseFVEProduction { get; set; } = true;

    [BindProperty]
    public List<DateTime> PredictedCostTimeStamps { get; set; } = [];
    [BindProperty]
    public List<float> PredictedCost { get; set; } = [];
    public async Task OnGet() {

        DataDate = DateTime.Now;
        DataDate = new DateTime(2026, 1, 27);

        await GetData();
    }

    public async Task OnPost()
    {
        await GetData();
        logger.LogError(UseFVEProduction.ToString());
    }

    private async Task GetData()
    {
        var spotData = await spot.GetSoapData(DataDate);
        SpotTimeStamps = spotData?.Select(x => x.DateTime).ToList() ?? [];
        SpotPrice = spotData?.Select(x => (float)x.Value).ToList() ?? [];

        var fveForecast = await forecastService.GetTomorrowWatthours();
        //var fveForecast = fveForecastService.GetPrediction(DataDate);
        FVEPredictionTimeStamps = fveForecast?.Select(x => x.DateTime).ToList() ?? [];
        FVEPrediction = fveForecast?.Select(x => (float)x.Value).ToList() ?? [];

        var consumptionPred = await predictionService.ConsumptionPrediction();
        ConsumptionPredictionTimeStamps = DataFilterHelper.GetHourlyDateTimes(DataDate);
        ConsumptionPrediction = ConsumptionPredictionTimeStamps.Select(x => (float)consumptionPred/24).ToList();

        PredictedCostTimeStamps = DataFilterHelper.GetHourlyDateTimes(DataDate);
        PredictedCost = [];
        for (int i = 0; i < PredictedCostTimeStamps.Count; i++)
        {
            var consumption = ConsumptionPrediction[i];
            var spot = SpotPrice[i] / 1000; // From EUR/MWh to EUR/kWh
            var fveProduction = UseFVEProduction ? FVEPrediction[i] / 10 : 0;

            PredictedCost.Add((consumption - fveProduction) * spot);
        }
    }

}
