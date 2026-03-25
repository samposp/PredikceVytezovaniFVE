using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.MeteoSource;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PredikceVytěžováníFVE.Services {
    public class OpenMeteoService(IServiceProvider serviceProvider, ILogger<SpotSoapService> logger, ConfigurationService configuration) {
        const string host = "api.open-meteo.com/v1/forecast";

        static private HttpClient client = new HttpClient();

        private string GetUri(string latitude, string longitude) {
            var uriBuilder = new UriBuilder(host);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["latitude"] = latitude;
            query["longitude"] = longitude;
            query["hourly"] = "temperature_2m";
            query["timezone"] = "auto";
            query["forecast_days"] = "2";
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        async public Task<List<TimeValuePair>?> GetTomorrowTemperature(DateTime date) {

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();
            var dbData = db.WeatherForecastData.Where(db => db.TimeStamp.Date == date.Date).ToList();
            if (dbData != null && dbData.Count == 24)
            {
                logger.LogInformation("Temperature prediction data retrieved from DB");
                return [.. dbData.Select(ConverterHelper.ToTimeValuePair)];
            }

            string latitude = configuration.Settings.Fve.Latitude ?? throw new Exception("Missing latitude");
            string longitude = configuration.Settings.Fve.Longitude ?? throw new Exception("Missing logitude");

            string uri = GetUri(latitude, longitude);
            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode) {
                OpenMeteoTemperature? jResponse = await response.Content.ReadFromJsonAsync<OpenMeteoTemperature>();
                if (jResponse == null)
                    throw new HttpRequestException("No content found");

                List<TimeValuePair> timeList = [];
                for (int i=0; i < jResponse.hourly.time.Count; i++)
                {
                    var timestamp = DateTime.Parse(jResponse.hourly.time[i]);
                    if (timestamp.Date == date.Date)
                    {
                        var temperature = (decimal) jResponse.hourly.temperature_2m[i];
                        timeList.Add(new TimeValuePair(timestamp, temperature));
                    }

                }

                logger.LogInformation("Spot data retrieved from SOAP");
                var boData = timeList.Select(ConverterHelper.ToWeatherForecastBo);
                await db.WeatherForecastData.AddRangeAsync(boData);
                await db.SaveChangesAsync();
                return timeList;
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
    }
}
