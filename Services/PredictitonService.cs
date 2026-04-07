using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Backtest;
using PredikceVytěžováníFVE.BackTest;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;

namespace PredikceVytěžováníFVE.Services;
public class PredictitonService(ILogger<PredictitonService> logger, SpotSoapService soapClient, ForecastService forecastService, ConfigurationService config, ConcumptionPredictionService consumptionService, MqttDataService mqttData, BatteryMilpOptimizationService batteryOptimizationService, IServiceProvider serviceProvider, PVForecastService oldFveService)
{
    public DateTime DataDate { get; set; }
    public List<TimeValuePair> SpotData { get; set; } = [];
    public List<TimeValuePair> FVEPrediction { get; set; } = [];
    public List<TimeValuePair> ConsumptionPrediction { get; set; } = [];
    public List<TimeValuePair> PredictedCost { get; set; } = [];
    public List<TimeValuePair> PredictedBattery { get; set; } = [];

    public async Task BackTest()
    {
        DateTime start = new DateTime(2026, 2, 10);
        DateTime end = new DateTime(2026, 2, 20);
        string resultFile = "backtestResult.csv";

        var backtestService = new BatteryBacktestService(batteryOptimizationService);
        List<HistoricalDayInput> input = new();
        for (DateTime date = start; date <= end; date = date.AddDays(1))
        {
            var spotData = await soapClient.GetSoapData(date) ?? [];
            var fvePrediction =  oldFveService.GetPrediction(date).ToList();
            var consumptionPrediction = await consumptionService.GetPrediction(date) ?? [];
            if (spotData.Count == 0 || fvePrediction.Count == 0 || consumptionPrediction.Count == 0)
            {
                logger.LogWarning("Missing data for date {date}, skipping backtest for this day", date.ToShortDateString());
                continue;
            }
            input.Add(new HistoricalDayInput
            {
                Date = DateOnly.FromDateTime(date),
                InitialBatteryPercent = 20,
                Spot = ConverterHelper.ToFloatList(spotData),
                Fve = ConverterHelper.ToFloatList(fvePrediction),
                Consumption = ConverterHelper.ToFloatList(consumptionPrediction)
            });
        }

        var result = backtestService.Run(input);
        BacktestCsvWriter.SaveDayResults(resultFile, result.Days);
    }

    public async Task Predict(DateTime date)
    {
        DataDate = date;
        SpotData = await soapClient.GetSoapData(date) ?? [];

        FVEPrediction = await forecastService.GetWatthours(date);

        ConsumptionPrediction = await consumptionService.GetPrediction(date) ?? [];

        //float batteryInitial = mqttData.GetLastBattery() ?? 20;
        float batteryInitial = 20;

        List<float> spotList = ConverterHelper.ToFloatList(SpotData); // From EUR/MWh to EUR/kWh
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

        //PredictedBattery = ConverterHelper.ToHourlyList(batteryInfo.BatteryCapacity, date);

        //List<float> predictedList = [];
        //for (int i = 0; i < SpotData.Count; i++)
        //{
        //    var consumption = consumptionList[i];
        //    var spot = spotList[i];
        //    var fveProduction = fveList[i];
        //    var batteryCharge = batteryInfo.BatteryDelta[i];

        //    predictedList.Add((consumption - fveProduction + batteryCharge) * spot);
        //}
        //PredictedCost = ConverterHelper.ToHourlyList(predictedList, date);
    }

    //private PredictedData GetOptimalAlg(float initialBattery, List<float> fve, List<float> consumption, List<float> spot)
    //{
    //    // analyze spot data 

    //    var maxCharge = 0;
    //    // get first charge
    //    var surplus = GetFVESurplus(fve, consumption);
    //    var start = DataDate.Date; // 0:00
    //    var end = DataDate.Date.AddHours(12); // 12:00
    //    var firstCharge = FindOptimalChargeTime(start, end, initialBattery, maxCharge);

    //    return new();
    //}

    private PredictedData GetOptimalBatteryCharge(float initialBattery, List<float> fve, List<float> consumption, List<float> spot)
    {
        int minTimeToCharge = 3;
        int latestTimeToCharge = 20;
        int latestTimeToDischarge = 22;
        List<PredictedData> batteryInfo = new();
        for (int i = 0; i < latestTimeToCharge; i++)
        {
            for (int j = i + minTimeToCharge; j < latestTimeToDischarge; j++)
            {
                List<int> charge = new() { i };
                List<int> discharge = new() { j };
                List<int> chargeTo = new() { 100 };
                (var batteryCharge, var batteryCapacity) = GetBattery(initialBattery, fve, consumption, charge, discharge);

                var price = GetPrice(consumption, spot, fve, batteryCharge);

                batteryInfo.Add(new(batteryCapacity, batteryCharge, price, charge, discharge, chargeTo));
            }
        }
        var minValue = batteryInfo.Min(x => x.PriceSum);
        return batteryInfo.Where(x => x.PriceSum == minValue).First();
    }

    private (List<float>, List<float>) GetBattery(float initialBattery, List<float> fve, List<float> consumption, List<int> chargeTime, List<int> dischargeTime)
    {
        float capacity = config.Settings.Battery.Capacity ?? throw new Exception("Missing battery capacity in config");
        float minRelative = ((float)(config.Settings.Battery.MinLevel ?? 20)) / 100;
        float maxRelative = ((float)(config.Settings.Battery.MaxLevel ?? 100)) / 100;
        float minCharge = minRelative * capacity;
        float maxCharge = maxRelative * capacity;
        List<float> batteryCapacity = new() { minCharge };
        List<float> batteryCharge = new() { 0 };
        bool charge = false;
        bool discharge = false;
        for (int i = 1; i < 24; i++)
        {
            float lastCapacity = batteryCapacity[i - 1];
            if (chargeTime.Contains(i))
            {
                charge = true;
                discharge = false;
            }
            if (dischargeTime.Contains(i))
            {
                discharge = true;
                charge = false;
            }

            float currentBattery = lastCapacity;
            float currentCharge = 0;

            if (charge)
            {
                if (lastCapacity < maxCharge)
                {
                    var val = fve[i];
                    currentBattery += val;
                    currentCharge = val;
                }
                else
                {
                    charge = false;
                }
            }

            if (discharge)
            {
                if (lastCapacity > minCharge)
                {
                    var cons = consumption[i] - fve[i];
                    cons = cons > 0 ? cons : 0;
                    currentBattery -= cons;
                    currentCharge = -cons;
                }
                else
                {
                    discharge = false;
                }
            }
            if (currentBattery > maxCharge)
                currentBattery = maxCharge;
            if (currentBattery < minCharge)
                currentBattery = minCharge;

            batteryCapacity.Add(currentBattery);
            batteryCharge.Add(currentCharge);
        }
        return (batteryCharge, batteryCapacity);
    }


    private float GetPrice(List<float> consumption, List<float> spot, List<float> fve, List<float> batteryCharge)
    {
        if (consumption.Count != spot.Count)
            throw new Exception($"Wrong consuption or spot count! consuption lenght:{consumption.Count}, spot length: ${spot.Count}");
        float sum = 0;
        for (int i = 0; i < spot.Count; i++)
        {
            var cons = consumption[i] - fve[i] + batteryCharge[i];
            sum += cons * spot[i];
        }
        return sum;
    }

    private async Task<DateTime> FindOptimalChargeTime(DateTime min, DateTime max, float start, float end)
    {
        if (min.Date != max.Date)
            throw new Exception("Min and max date must be the same");

        var chargeSpeed = config.Settings.Battery.ChargeSpeed ?? throw new Exception("Missing charge speed in config");
        var capacity = config.Settings.Battery.Capacity ?? throw new Exception("Missing battery capacity in config");

        var minutesCharge = (int)Math.Ceiling((capacity * (end - start)) / chargeSpeed);
        TimeSpan timeCharge = new(0, minutesCharge, 0);
        var lastStartCharge = max.Subtract(timeCharge);
        var spotData = await soapClient.GetSoapData(min.Date);
        if (spotData == null || spotData.Count == 0)
            throw new Exception("No spot data available for date: " + min.Date.ToShortDateString());

        spotData = spotData.Where(x => x.DateTime >= min && x.DateTime <= lastStartCharge).ToList();
        if (spotData.Count == 0)
            throw new Exception("No possible time to charge found");

        DateTime bestTime = new();
        float bestPrice = float.MaxValue;
        foreach (var data in spotData)
        {
            DateTime endCharge = data.DateTime.Add(timeCharge);
            var price = (float)spotData.Where(x => x.DateTime >= data.DateTime && x.DateTime < endCharge).Sum(x => x.Value);
            if (price < bestPrice)
            {
                bestPrice = price;
                bestTime = data.DateTime;
            }
        }
        return bestTime;
    }

    private List<float> GetFVESurplus(List<float> fve, List<float> consumption)
    {
        List<float> surplus = [];
        for (int i = 0; i < fve.Count; i++)
        {
            var val = fve[i] - consumption[i];
            surplus.Add(val > 0 ? val : 0);
        }
        return surplus;
    }

}

public record PredictedData(List<float> Capacity, List<float> Charge, float PriceSum, List<int> ChargeTime, List<int> DischargeTime, List<int> ChargeToCapacity);