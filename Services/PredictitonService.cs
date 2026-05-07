using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Backtest;
using PredikceVytěžováníFVE.BackTest;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;

namespace PredikceVytěžováníFVE.Services;
public class PredictitonService(SpotSoapService soapClient, ForecastService forecastService, ConcumptionPredictionService consumptionService, MqttDataService mqttData, BatteryMilpOptimizationService batteryOptimizationService, IServiceProvider serviceProvider)
{
    public DateTime DataDate { get; set; }
    public List<TimeValuePair> SpotData { get; set; } = [];
    public List<TimeValuePair> FVEPrediction { get; set; } = [];
    public List<TimeValuePair> ConsumptionPrediction { get; set; } = [];
    public List<TimeValuePair> PredictedCost { get; set; } = [];
    public List<TimeValuePair> PredictedBattery { get; set; } = [];

    public async Task<ControlPredictionBo> Predict(DateTime date)
    {
        DataDate = date;
        SpotData = await soapClient.GetSoapData(date) ?? [];

        FVEPrediction = await forecastService.GetForecast(date);

        ConsumptionPrediction = await consumptionService.GetPrediction(date) ?? [];

        float batteryInitial = mqttData.GetLastBattery() ?? 20;

        List<float> spotList = ConverterHelper.ToFloatList(SpotData);
        var fveList = ConverterHelper.ToFloatList(FVEPrediction);
        var consumptionList = ConverterHelper.ToFloatList(ConsumptionPrediction);

        var batteryInfo = batteryOptimizationService.Optimize(
            batteryInitial,
            fveList,
            consumptionList,
            spotList,
            date);

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();
        var prediction = ConverterHelper.ToControlPredictionBo(date, batteryInfo);

        var existing = await db.PredictedControlData
            .FirstOrDefaultAsync(x => x.TimeStamp == prediction.TimeStamp);

        if (existing is null)
        {
            await db.PredictedControlData.AddAsync(prediction);
        }
        else
        {
            existing.Capacity = prediction.Capacity;
            existing.TimeStamp = prediction.TimeStamp;
            existing.PriceSum = prediction.PriceSum;
            existing.Charge = prediction.Charge;
            existing.ChargeToCapacities = prediction.ChargeToCapacities;
            existing.ChargeTimes = prediction.ChargeTimes;
            existing.DischargeTimes = prediction.DischargeTimes;
            existing.GridBuy = prediction.GridBuy;
            existing.GridSell = prediction.GridSell;
        }   

        await db.SaveChangesAsync();
        return prediction;
    }

}