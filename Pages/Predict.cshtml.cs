using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Pages; 
public class PredictModel(PredictionDataService predictionService, PredictitonService runPredictionService) : PageModel
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
    public List<float> PredictedBuy { get; set; } = [];

    public List<float> PredictedSell { get; set; } = [];

    public List<float> PredictedBattery { get; set; } = [];
    public List<DateTime> PredictedBatteryTimeStamps { get; set; } = [];

    public string DateMessage { get; set; } = "";
    public async Task OnGet() {

        //await pred.BackTest();
        await GetData();
    }
    private async Task GetData()
    {
        DataDate = DateTime.Now.AddDays(1);
        bool isPrediction = await predictionService.GetPredition(DataDate);
        if (!isPrediction)
        {
            DataDate = DateTime.Now;
            bool isTodayPred = await predictionService.GetPredition(DataDate);
            if (isTodayPred)
                DateMessage = "Zobrazena predikce pro " + DataDate.ToShortDateString();
            else
            {
                DateMessage = "Nenalezena predikce pro dnešek ani zítřek";
                return;
            }
        }
        else
            DateMessage = "Zobrazena predikce pro " + DataDate.ToShortDateString();

        //DataDate = DateTime.Now.AddDays(-1);
        //await pred.Predict(DataDate);
        //await predictionService.GetPredition(DataDate);

        SpotTimeStamps = predictionService.SpotData?.Select(x => x.DateTime).ToList() ?? [];
        SpotPrice = predictionService.SpotData?.Select(x => (float)x.Value).ToList() ?? [];

        FVEPredictionTimeStamps = predictionService.FVEPrediction?.Select(x => x.DateTime).ToList() ?? [];
        FVEPrediction = predictionService.FVEPrediction?.Select(x => (float)((float)x.Value)).ToList() ?? [];

        ConsumptionPredictionTimeStamps = predictionService.ConsumptionPrediction?.Select(x => x.DateTime).ToList() ?? [];
        ConsumptionPrediction = predictionService.ConsumptionPrediction?.Select(x => (float)x.Value).ToList() ?? [];

        PredictedCostTimeStamps = predictionService.PredictedBuy?.Select(x => x.DateTime).ToList() ?? [];
        PredictedBuy = predictionService.PredictedBuy?.Select(x => (float)x.Value).ToList() ?? [];
        PredictedSell = predictionService.PredictedSell?.Select(x => (float)x.Value).ToList() ?? [];

        PredictedBattery = predictionService.PredictedBattery?.Select(x => (float)x.Value).ToList() ?? [];
        PredictedBatteryTimeStamps = predictionService.PredictedBattery?.Select(x => x.DateTime).ToList() ?? [];
    }

    //public async Task<IActionResult> OnPost()
    //{
    //    await runPredictionService.Predict(DateTime.Now);
    //    return RedirectToPage();
    //}
}
