using Google.OrTools.ConstraintSolver;
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

    public List<DateTime> SpotTimeStamps { get; set; } = [];
    public List<float> SpotPrice { get; set; } = [];
    public List<DateTime> FVEPredictionTimeStamps { get; set; } = [];
    public List<float> FVEPrediction { get; set; } = [];
    public List<DateTime> ConsumptionPredictionTimeStamps { get; set; } = [];
    public List<float> ConsumptionPrediction { get; set; } = [];
    public bool UseFVEProduction { get; set; } = true;
    public List<DateTime> PredictedCostTimeStamps { get; set; } = [];
    public List<float> PredictedBuy { get; set; } = [];
    public List<float> PredictedSell { get; set; } = [];
    public List<float> PredictedBattery { get; set; } = [];
    public List<DateTime> PredictedBatteryTimeStamps { get; set; } = [];

    public string BatteryMessage { get; set; } = "";
    public string DateMessage { get; set; } = "";

    public bool CanRerun = false;
    public async Task OnGet()
    {

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
                DateMessage = "Zobrazena predikce pro " + DataDate.ToString("dd.MM.yyyy");
            else
            {
                DateMessage = "Nenalezena predikce pro dnešek ani zítřek";
                return;
            }
        }
        else
            DateMessage = "Zobrazena predikce pro " + DataDate.ToString("dd.MM.yyyy");

        //DataDate = DateTime.Now.AddDays(-1);
        //await pred.Predict(DataDate);
        //await predictionService.GetPredition(DataDate);

        SetBatteryMessage();

        //await send.SendData(predictionService.PrediectedControlData);

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
    private void SetBatteryMessage()
    {
        BatteryMessage = $"Nabíjení baterie ze sítě: ";
        for (int i = 0; i < predictionService.PredictedControlData?.ChargeTimes?.Count; i++)
        {
            var time = predictionService.PredictedControlData.ChargeTimes[i];
            var charge = predictionService.PredictedControlData.ChargeToCapacities?[i];
            if (charge != null)
            {
                BatteryMessage += $"{time.ToString("HH:mm")} - {charge.Value}%, ";
            }
        }
        BatteryMessage += "Vybíjení baterie: ";
        for (int i = 0; i < predictionService.PredictedControlData?.DischargeTimes?.Count; i++)
        {
            var time = predictionService.PredictedControlData.DischargeTimes[i];
            BatteryMessage += $"{time.ToString("HH:mm")}, ";
        }
        BatteryMessage += $" Celková cena za energie ze sítě: {predictionService.PredictedControlData?.PriceSum?.ToString("F2")} CZK";
    }
    public async Task<IActionResult> OnPost()
    {
        if (CanRerun)
            await runPredictionService.Predict(DateTime.Now);
        return RedirectToPage();
    }
}
