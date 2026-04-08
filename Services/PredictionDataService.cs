using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using System.Reflection.Metadata.Ecma335;

namespace PredikceVytěžováníFVE.Services;

public class PredictionDataService(ILogger<PredictitonService> logger, SpotSoapService soapClient, ForecastService forecastService, ConcumptionPredictionService consumptionService, FVEDbContext db)
{
    public DateTime DataDate { get; set; }
    public List<TimeValuePair> SpotData { get; set; } = [];
    public List<TimeValuePair> FVEPrediction { get; set; } = [];
    public List<TimeValuePair> ConsumptionPrediction { get; set; } = [];
    public List<TimeValuePair> PredictedBuy { get; set; } = [];
    public List<TimeValuePair> PredictedSell { get; set; } = [];
    public List<TimeValuePair> PredictedBattery { get; set; } = [];
    public ControlPredictionBo? PredictedControlData { get; set; }
    public async Task<bool> GetPredition(DateTime date)
    {
        DataDate = date;
        PredictedControlData = db.PredictedControlData.Where(x => x.TimeStamp.Date == date.Date).FirstOrDefault();
        if (PredictedControlData == null)
        {
            logger.LogWarning("Can not find predicted data for date: {date}", date.ToShortDateString());
            return false;
        }
        SpotData = await soapClient.GetSoapData(date) ?? [];
        FVEPrediction = await forecastService.GetWatthours(date);
        ConsumptionPrediction = await consumptionService.GetPrediction(date, true) ?? [];

        var batteryChargeList = PredictedControlData.Charge!;
        try
        {
            PredictedBattery = ConverterHelper.ToQuarterHourlyList(PredictedControlData.Capacity, date);
            PredictedBuy = ConverterHelper.ToQuarterHourlyList(PredictedControlData.GridBuy, date);
            PredictedSell = ConverterHelper.ToQuarterHourlyList(PredictedControlData.GridSell, date);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            try
            {
                if (PredictedBattery.Count == 0)
                {
                    logger.LogInformation("Trying to get hourly values");
                    PredictedBattery = ConverterHelper.ToHourlyList(PredictedControlData.Capacity, date);
                }
                if (PredictedBuy.Count == 0)
                {
                    logger.LogInformation("Trying to get hourly values");
                    PredictedBuy = ConverterHelper.ToHourlyList(PredictedControlData.GridBuy, date);
                }
                if (PredictedSell.Count == 0)
                {
                    logger.LogInformation("Trying to get hourly values");
                    PredictedSell = ConverterHelper.ToHourlyList(PredictedControlData.GridSell, date);
                }
            }
            catch (Exception ex2)
            {
                logger.LogError(ex2.Message);

            }

        }



        return true;
    }

}
