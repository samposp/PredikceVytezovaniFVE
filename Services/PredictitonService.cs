using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;

namespace PredikceVytěžováníFVE.Services; 
public class PredictitonService(ILogger<PredictitonService> logger, SpotSoapService soapClient, ForecastService forecastService, ConfigurationService config, ConcumptionPredictionService consumptionService, MqttDataService mqttData) {
    public DateTime DataDate { get; set; }
    public List<TimeValuePair> SpotData { get; set; } = [];
    public List<TimeValuePair> FVEPrediction { get; set; } = [];
    public List<TimeValuePair> ConsumptionPrediction { get; set; } = [];
    public List<TimeValuePair> PredictedCost { get; set; } = [];
    public List<TimeValuePair> PredictedBattery { get; set; } = [];

    public async Task Predict(DateTime date)
    {
        DataDate = date;
        SpotData = await soapClient.GetHourlyAverageSoapData(date) ?? [];

        FVEPrediction = await forecastService.GetWatthours(date);

        ConsumptionPrediction = await consumptionService.GetPrediction(date) ?? [];

        float batteryInitial = mqttData.GetLastBattery() ?? 20;

        var spotList = ConverterHelper.ToFloatList(SpotData);
        var fveList = ConverterHelper.ToFloatList(FVEPrediction);
        var consumptionList = ConverterHelper.ToFloatList(ConsumptionPrediction);

        var batteryInfo = GetOptimalBatteryCharge(batteryInitial, fveList, consumptionList, spotList);

        float capacity = config.Settings.Battery.Capacity ?? 100;
        PredictedBattery = ConverterHelper.ToHourlyList(batteryInfo.Capacity, date);

        List<float> predictedList = [];
        for (int i = 0; i < SpotData.Count; i++)
        {
            var consumption = consumptionList[i];
            var spot = spotList[i] / 1000; // From EUR/MWh to EUR/kWh
            var fveProduction = fveList[i];
            var batteryCharge = batteryInfo.Charge[i];

            predictedList.Add((consumption - fveProduction + batteryCharge) * spot);
        }
        PredictedCost = ConverterHelper.ToHourlyList(predictedList, date);
    }

    private PredictedData GetOptimalBatteryCharge(float initialBattery, List<float> fve, List<float> consumption, List<float> spot)
    {
        int minTimeToCharge = 3;
        int latestTimeToCharge = 20;
        int latestTimeToDischarge = 22;
        List<PredictedData> batteryInfo = new(); 
        for (int i=0; i < latestTimeToCharge; i++)
        {
            for (int j = i + minTimeToCharge; j < latestTimeToDischarge; j++)
            {
                List<int> charge = new() { i };
                List<int> discharge = new() { j };
                List<int> chargeTo = new() { 100 };
                (var batteryCharge, var batteryCapacity)= GetBattery(initialBattery, fve, consumption, charge, discharge);

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
        List<float> batteryCapacity = new(){ minCharge };
        List<float> batteryCharge = new() { 0 };
        bool charge = false;
        bool discharge = false;
        for(int i=1; i < 24; i++)
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
            throw new Exception($"Wrong consuption or spout count! consuption lenght:{consumption.Count}, spot length: ${spot.Count}");
        float sum = 0;
        for (int i=0; i < spot.Count; i++)
        {
            var cons = consumption[i] - fve[i] + batteryCharge[i];
            sum += cons * spot[i];
        }
        return sum;
    }
}

public record PredictedData(List<float> Capacity, List<float> Charge, float PriceSum, List<int> ChargeTime, List<int> DischargeTime, List<int> ChargeToCapacity);