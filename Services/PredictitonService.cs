using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Models;
using PublicOTEService;
using SQLitePCL;

namespace PredikceVytěžováníFVE.Services; 
public class PredictitonService {
    public List<double> HourlyEnergy = new();
    public List<double> HourlyPrice = new();

    private readonly OpenMeteoService meteoService;
    private readonly SpotSoapService soapClient;
    private readonly PVForecastService fveforecastService;
    private readonly ILogger<PredictitonService> logger;

    public PredictitonService(SpotSoapService soapClient, PVForecastService forecastService, ILogger<PredictitonService> logger, OpenMeteoService meteoService)
    {
        this.soapClient = soapClient;
        this.fveforecastService = forecastService;
        this.logger = logger;
        this.meteoService = meteoService;
    }

    public async Task<double> ConsumptionPrediction(DateTime date) {

        var meteoResponse = await meteoService.GetTomorrowTemperature(date);

        // from linear regression
        //double coef_min = -1.48756492;
        //double coef_max = -0.52444613;
        //double intercept = 60.11416895;
        //return (intercept + coef_min * minTemp + coef_max * maxTemp);
        return 1;
    }

    public async Task Predict()
    {
        await Hourly();
    }
    public async Task Hourly() {
        
        DateTime tomorrow = DateTime.Now.AddDays(1);
        IEnumerable<TimeValuePair> pvForecast = await fveforecastService.GetTommorowPrediciton();
        //var fveForecast = await forecastService.GetTomorrowWatthours();
        List<TimeValuePair>? spotData = await soapClient.GetHourlyAverageSoapData(tomorrow);
        if (spotData == null)
        {
            logger.LogError("Spot data is null, cannot predict");
            return;
        }

        double pred = await ConsumptionPrediction(tomorrow);
        double hourPred = pred / 24;
        for (int i = 0; i < 24; i++) {
            HourlyPrice.Add(hourPred * (double)spotData[i].Value/1000); // EUR/kWh
        }


        var spotTimeStamps = spotData?.Select(x => x.DateTime).ToList() ?? [];
        var spotPrice = spotData?.Select(x => (float)x.Value).ToList() ?? [];

        //var fveForecast = fveForecastService.GetPrediction(DataDate);
        //FVEPredictionTimeStamps = fveForecast?.Select(x => x.DateTime).ToList() ?? [];
        //FVEPrediction = fveForecast?.Select(x => (float)x.Value / 1000).ToList() ?? [];

        //var consumptionPred = await predictionService.ConsumptionPrediction();
        //ConsumptionPredictionTimeStamps = DataFilterHelper.GetHourlyDateTimes(DataDate);
        //ConsumptionPrediction = ConsumptionPredictionTimeStamps.Select(x => (float)consumptionPred/24).ToList();

        //var batteryInitial = 20;


        //PredictedCostTimeStamps = DataFilterHelper.GetHourlyDateTimes(DataDate);
        //PredictedCost = [];
        //for (int i = 0; i < PredictedCostTimeStamps.Count; i++)
        //{
        //    var consumption = ConsumptionPrediction[i];
        //    var spot = SpotPrice[i] / 1000; // From EUR/MWh to EUR/kWh
        //    var fveProduction = UseFVEProduction ? FVEPrediction[i] : 0;

        //    PredictedCost.Add((consumption - fveProduction) * spot);
        //}
    }

}
