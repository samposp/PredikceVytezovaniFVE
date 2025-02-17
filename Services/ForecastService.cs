using Microsoft.AspNetCore.Http.Extensions;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.Forecast;
using System.IO;
using System.Net.Http.Headers;
using System.Xml.Serialization;

namespace PredikceVytěžováníFVE.Services {
    public class ForecastService {

        private static readonly string host = "https://api.forecast.solar/estimate/";
        static private HttpClient client = new HttpClient();

        public ForecastService()
        {
            client.BaseAddress = new Uri(host);
        }

        public async Task<IEnumerable<TimeValuePair>> GetWatthours(string latitude, string longitude, string peakPower, string declination = "0", string azimuth = "0") {

            List<string> path = new() {
                "watthours",
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
