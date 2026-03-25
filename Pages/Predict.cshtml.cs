using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Pages; 
public class PredictModel(ILogger<PredictModel> logger, PVForecastService fveForecastService, SpotSoapService spot, PredictitonService predictionService, ForecastService forecastService, ConfigurationService config, ConcumptionPredictionService consumptionService, MqttDataService mqttData) : PageModel
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
        //DataDate = new DateTime(2026, 1, 27);

        await GetData();
    }

    public async Task OnPost()
    {
        await GetData();
        logger.LogError(UseFVEProduction.ToString());
    }

    private async Task GetData()
    {
        var spotData = await spot.GetHourlyAverageSoapData(DataDate);
        SpotTimeStamps = spotData?.Select(x => x.DateTime).ToList() ?? [];
        SpotPrice = spotData?.Select(x => (float)x.Value).ToList() ?? [];

        var fveForecast = await forecastService.GetTomorrowWatthours();
        //var fveForecast = fveForecastService.GetPrediction(DataDate);
        FVEPredictionTimeStamps = fveForecast?.Select(x => x.DateTime).ToList() ?? [];
        FVEPrediction = fveForecast?.Select(x => (float)((float)x.Value)/ 1000).ToList() ?? [];


        var consumptionPred = await consumptionService.GetPrediction(DataDate);
        //var consumptionPred = await predictionService.ConsumptionPrediction(DataDate);
        ConsumptionPredictionTimeStamps = consumptionPred?.Select(x => x.DateTime).ToList() ?? [];
        ConsumptionPrediction = consumptionPred?.Select(x => (float)x.Value/1000).ToList() ?? [];

        float batteryInitial = mqttData.GetLastBattery() ?? 20;

        var batteryInfo = GetOptimalBatteryCharge(batteryInitial, FVEPrediction, ConsumptionPrediction, SpotPrice);

        float capacity = config.Settings.Battery.Capacity ?? 100;
        PredictedBattery = batteryInfo.capacity.Select(x => x * 100 / capacity).ToList(); 

        PredictedCostTimeStamps = DataFilterHelper.GetHourlyDateTimes(DataDate);
        PredictedCost = [];
        for (int i = 0; i < PredictedCostTimeStamps.Count; i++)
        {
            var consumption = ConsumptionPrediction[i];
            var spot = SpotPrice[i] / 1000; // From EUR/MWh to EUR/kWh
            var fveProduction = UseFVEProduction ? FVEPrediction[i] : 0;
            var batteryCharge = batteryInfo.charge[i];

            PredictedCost.Add((consumption - fveProduction + batteryCharge) * spot);
        }
    }

    private BatteryInfo GetOptimalBatteryCharge(float initialBattery, List<float> fve, List<float> consumption, List<float> spot)
    {
        int minTimeToCharge = 3;
        int latestTimeToCharge = 20;
        int latestTimeToDischarge = 22;
        List<BatteryInfo> batteryInfo = new(); 
        for (int i=0; i < latestTimeToCharge; i++)
        {
            for (int j = i + minTimeToCharge; j < latestTimeToDischarge; j++)
            {
                List<int> charge = new() { i };
                List<int> discharge = new() { j };
                (var batteryCharge, var batteryCapacity)= GetBattery(initialBattery, fve, consumption, charge, discharge);

                var price = GetPrice(consumption, spot, fve, batteryCharge);

                batteryInfo.Add(new(batteryCapacity, batteryCharge, price, charge, discharge));
            }
        }
        var minValue = batteryInfo.Min(x => x.priceSum);
        return batteryInfo.Where(x => x.priceSum == minValue).First();
    }

    record BatteryInfo(List<float> capacity, List<float> charge, float priceSum, List<int> chargeTime, List<int> dischargeTime);

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
