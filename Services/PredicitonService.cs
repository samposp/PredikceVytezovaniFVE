using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Models;
using PublicOTEService;

namespace PredikceVytěžováníFVE.Services; 
public class PredicitonService(SpotSoapService soapClient, ILogger<PredicitonService> logger) {
    public List<double> HourlyEnergy = new();
    public List<double> HourlyPrice = new();

    private readonly OpenMeteoService meteoService = new();

    private static double ProducionPrediction(double minTemp, double maxTemp) {
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
        string latitude = "50.79";
        string longitude = "15.145";
        
        DateTime tomorrow = DateTime.Now.AddDays(1);
        List<TimeValuePair>? spot = await soapClient.GetSoapData(tomorrow);
        if (spot == null)
        {
            logger.LogError("Spot data is null, cannot predict");
            return;
        }

        OpenMeteoTemperature meteoResponse = await meteoService.GetTemperature(latitude, longitude);
        double minTemp = meteoResponse.daily.temperature_2m_min[0];
        double maxTemp = meteoResponse.daily.temperature_2m_max[0];
        double pred = ProducionPrediction(minTemp, maxTemp);
        double hourPred = pred / 24;
        for (int i = 0; i < 24; i++) {
            HourlyPrice.Add(hourPred * (double)spot[i].Value/1000);
        }
    }

}
