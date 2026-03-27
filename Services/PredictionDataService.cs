using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using System.Reflection.Metadata.Ecma335;

namespace PredikceVytěžováníFVE.Services;

public class PredictionDataService(ILogger<PredictitonService> logger, SpotSoapService soapClient, ForecastService forecastService, ConcumptionPredictionService consumptionService, FVEDbContext db)
{
    public DateTime DataDate { get; set; }
    public List<TimeValuePair> SpotData { get; set; } = [];
    public List<TimeValuePair> FVEPrediction { get; set; } = [];
    public List<TimeValuePair> ConsumptionPrediction { get; set; } = [];
    public List<TimeValuePair> PredictedCost { get; set; } = [];
    public List<TimeValuePair> PredictedBattery { get; set; } = [];

    public async Task<bool> GetPredition(DateTime date)
    {
        DataDate = date;
        SpotData = await soapClient.GetHourlyAverageSoapData(date) ?? [];
        FVEPrediction = await forecastService.GetWatthours(date);
        ConsumptionPrediction = await consumptionService.GetPrediction(date) ?? [];
        var predictionData = db.PredictedControlData.Where(x => x.TimeStamp.Date == date.Date).FirstOrDefault();
        if (predictionData == null)
        {
            logger.LogWarning("Can not find predicted data for date: {date}", date.ToShortDateString());
            return false;
        }

        var batteryChargeList = predictionData.Charge!;
        PredictedBattery = ConverterHelper.ToHourlyList(predictionData.Capacity!, date);

        var spotList = ConverterHelper.ToFloatList(SpotData);
        var fveList = ConverterHelper.ToFloatList(FVEPrediction);
        var consumptionList = ConverterHelper.ToFloatList(ConsumptionPrediction);

        List<float> predictedList = [];
        for (int i = 0; i < SpotData.Count; i++)
        {
            var consumption = consumptionList[i];
            var spot = spotList[i] / 1000; // From EUR/MWh to EUR/kWh
            var fveProduction = fveList[i];
            var batteryCharge = batteryChargeList[i];

            predictedList.Add((consumption - fveProduction + batteryCharge) * spot);
        }
        PredictedCost = ConverterHelper.ToHourlyList(predictedList, date);
        return true;
    }

}
