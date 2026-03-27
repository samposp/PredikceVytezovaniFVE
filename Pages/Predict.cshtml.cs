using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Pages; 
public class PredictModel(ILogger<PredictModel> logger, PredictionDataService predictionService) : PageModel
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

    public List<float> PredictedBattery { get; set; } = [];
    public async Task OnGet() {

        DataDate = DateTime.Now.AddDays(1);
        await GetData();
    }

    public async Task OnPost()
    {
        await GetData();
    }

    private async Task GetData()
    {
        await predictionService.GetPredition(DataDate);

        SpotTimeStamps = predictionService.SpotData?.Select(x => x.DateTime).ToList() ?? [];
        SpotPrice = predictionService.SpotData?.Select(x => (float)x.Value).ToList() ?? [];

        FVEPredictionTimeStamps = predictionService.FVEPrediction?.Select(x => x.DateTime).ToList() ?? [];
        FVEPrediction = predictionService.FVEPrediction?.Select(x => (float)((float)x.Value)/ 1000).ToList() ?? [];

        ConsumptionPredictionTimeStamps = predictionService.ConsumptionPrediction?.Select(x => x.DateTime).ToList() ?? [];
        ConsumptionPrediction = predictionService.ConsumptionPrediction?.Select(x => (float)x.Value/1000).ToList() ?? [];

        PredictedCostTimeStamps = predictionService.PredictedCost?.Select(x => x.DateTime).ToList() ?? [];
        PredictedCost = predictionService.PredictedCost?.Select(x => (float)x.Value).ToList() ?? [];
        PredictedBattery = predictionService.PredictedBattery?.Select(x => (float)x.Value).ToList() ?? [];
    }
}
