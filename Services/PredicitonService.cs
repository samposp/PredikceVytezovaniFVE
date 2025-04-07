using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Models;
using ServiceReference1;

namespace PredikceVytěžováníFVE.Services {
    public class PredicitonService {
        public List<double> HourlyEnergy = new();
        public List<double> HourlyPrice = new();

        private readonly OpenMeteoService meteoService = new();
        private readonly PublicDataServiceSoapClient soapClient = new();
        private readonly FVEDbContext _database;

        public PredicitonService(FVEDbContext database)
        {
            _database = database;
        }

        private static double ProducionPrediction(double minTemp, double maxTemp) {
            // from linear regression
            double coef_min = -1.48756492;
            double coef_max = -0.52444613;
            double intercept = 60.11416895;
            return (intercept + coef_min * minTemp + coef_max * maxTemp);
        }

        public async Task Hourly() {
            string latitude = "50.79";
            string longitude = "15.145";

            //List<double> spot = await GetSpot();
            DateTime tomorrow = DateTime.Now.AddDays(-1);
            List<double> spot = _database.spotData.Where(x => x.TimeStamp == tomorrow).Select(x=>x.Value).ToList();

            OpenMeteoTemperature meteoResponse = await meteoService.GetTemperature(latitude, longitude);
            double minTemp = meteoResponse.daily.temperature_2m_min[0];
            double maxTemp = meteoResponse.daily.temperature_2m_max[0];
            double pred = ProducionPrediction(minTemp, maxTemp);
            double hourPred = pred / 24;
            for (int i = 0; i < 24; i++) {
                HourlyPrice.Add(hourPred * spot[i]/1000);
            }
        }

        private async Task<List<double>> GetSpot() {
            DateTime tomorrow = DateTime.Now.AddDays(0);
            GetDamPriceEResponse damPrice = await soapClient.GetDamPriceEAsync(tomorrow, tomorrow, 1, 24, false);
            return damPrice.Result.Select(x => (double)x.Price).ToList();
        }

    }
}
