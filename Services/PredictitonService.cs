using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Models;
using PublicOTEService;
using SQLitePCL;

namespace PredikceVytěžováníFVE.Services; 
public class PredictitonService {
    public List<double> HourlyEnergy = new();
    public List<double> HourlyPrice = new();

    private readonly OpenMeteoService meteoService = new();
    private readonly SpotSoapService soapClient;
    private readonly PVForecastService forecastService;
    private readonly ILogger<PredictitonService> logger;

    private readonly string _latitude;
    private readonly string _longitude;

    public PredictitonService(SpotSoapService soapClient, PVForecastService forecastService, ILogger<PredictitonService> logger, ConfigurationService configuration)
    {
        this.soapClient = soapClient;
        this.forecastService = forecastService;
        this.logger = logger;
        _latitude = configuration.Settings.Fve.Latitude ?? throw new Exception("Missing latitude");
        _longitude = configuration.Settings.Fve.Longitude ?? throw new Exception("Missing longitude");
    }

    public async Task<double> ConsumptionPrediction() {

        OpenMeteoTemperature meteoResponse = await meteoService.GetTomorrowTemperature(_latitude, _longitude);
        double minTemp = meteoResponse.daily.temperature_2m_min[0];
        double maxTemp = meteoResponse.daily.temperature_2m_max[0];

        // from linear regression
        double coef_min = -1.48756492;
        double coef_max = -0.52444613;
        double intercept = 60.11416895;
        return (intercept + coef_min * minTemp + coef_max * maxTemp);
    }

    public async Task Predict()
    {
        await Hourly();
    }
    public async Task Hourly() {
        
        DateTime tomorrow = DateTime.Now.AddDays(1);
        IEnumerable<TimeValuePair> pvForecast = await forecastService.GetTommorowPrediciton();
        List<TimeValuePair>? spot = await soapClient.GetSoapData(tomorrow);
        if (spot == null)
        {
            logger.LogError("Spot data is null, cannot predict");
            return;
        }

        double pred = await ConsumptionPrediction();
        double hourPred = pred / 24;
        for (int i = 0; i < 24; i++) {
            HourlyPrice.Add(hourPred * (double)spot[i].Value/1000);
        }
    }

}
