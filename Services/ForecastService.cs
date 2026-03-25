using Microsoft.AspNetCore.Http.Extensions;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.Forecast;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Xml.Serialization;

namespace PredikceVytěžováníFVE.Services {
    public class ForecastService {

        private static readonly string host = "https://api.forecast.solar/estimate/";
        private readonly IServiceProvider serviceProvider;

        string _latitude;
        string _longitude;
        string _azimuth;
        string _peakPower;
        string _declination;

        public ForecastService(IServiceProvider serviceProvider, ConfigurationService configuration)
        {
            this.serviceProvider = serviceProvider;
            _latitude = configuration.Settings.Fve.Latitude ?? throw new Exception("Missing latitude");
            _longitude = configuration.Settings.Fve.Longitude ?? throw new Exception("Missing logitude");
            _azimuth = configuration.Settings.Fve.Azimuth.ToString() ?? "0";
            _peakPower = configuration.Settings.Fve.PeakPower.ToString()?? throw new Exception("Missing preak power");
            _declination = configuration.Settings.Fve.Declination.ToString() ?? "0";
        }

        public async Task<List<TimeValuePair>> GetTomorrowWatthours() 
        {    
            DateTime tomorrow = DateTime.Now.AddDays(1);
            tomorrow = DateTime.Now;
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();
            var storedData = db.ForecastSolarData.Where(p => p.TimeStamp.Date == tomorrow.Date);
            if (storedData.Any())
                return storedData.Select(d => new TimeValuePair(d.TimeStamp, d.Value)).ToList();

            List<string> path = new() {
                "watthours",
                _latitude,
                _longitude,
                _declination,
                _azimuth,
                _peakPower
            };

            Dictionary<string, string> queryParams = new() {
                {"time", "utc" }
            };

            string url = Helpers.UriHelper.CreateUrl(path, queryParams);

            HttpClient client = new();
            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string forecastResponse = await response.Content.ReadAsStringAsync();
                WatthourResponse? deserialized;
                XmlSerializer serializer = new XmlSerializer(typeof(WatthourResponse));
                using (TextReader reader = new StringReader(forecastResponse)) {
                    deserialized = (WatthourResponse?)serializer.Deserialize(reader);
                }

                if (deserialized == null)
                    throw new HttpRequestException("No content found");



                var hourList = DataFilterHelper.GetHourlyDateTimes(tomorrow);
                var cummulativeList = deserialized.Result.Data.ToList();
                decimal lastValue = 0;
                List<TimeValuePair> result = [];
                foreach(var hour in hourList)
                {
                    decimal value = 0;
                    var data = cummulativeList.Where(x => x.Key.Date == hour.Date && x.Key.Hour == hour.Hour).LastOrDefault();
                    if (data != null)
                    {
                        value = data.Value - lastValue;
                        lastValue = data.Value;
                    }
                    result.Add(new (hour, value));
                }

                db.ForecastSolarData.AddRange(result.Select(r => new Models.DB.ForecastSolarBo {
                    TimeStamp = r.DateTime,
                    Value = r.Value
                }));
                await db.SaveChangesAsync();

                return result;
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }

        public async Task<IEnumerable<TimeValuePair>> GetWatthoursDay(string latitude, string longitude, string peakPower, string declination = "0", string azimuth = "0") {

            List<string> path = new() {
                "watthours",
                "day",
                latitude,
                longitude,
                declination,
                azimuth,
                peakPower
            };

            Dictionary<string, string> queryParams = new() {
                {"time", "utc" }
            };

            string url = Helpers.UriHelper.CreateUrl(path, queryParams);
            HttpClient client = new();
            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string forecastResponse = await response.Content.ReadAsStringAsync();
                WatthourResponse? deserialized;
                XmlSerializer serializer = new XmlSerializer(typeof(WatthourResponse));
                using (TextReader reader = new StringReader(forecastResponse)) {
                    deserialized = (WatthourResponse?)serializer.Deserialize(reader);
                }

                if (deserialized == null)
                    throw new HttpRequestException("No content found");

                return deserialized.Result.Data.Select(x => new TimeValuePair(x.Key, x.Value));
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
    }
}
