using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.MeteoSource;
using System.Web;

namespace PredikceVytěžováníFVE.Services {
    public class OpenMeteoService {
        const string host = "api.open-meteo.com/v1/forecast";

        static private HttpClient client = new HttpClient();

        private string GetUri(string latitude, string longitude) {
            var uriBuilder = new UriBuilder(host);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["latitude"] = latitude;
            query["longitude"] = longitude;
            query["daily"] = "temperature_2m_max,temperature_2m_min";
            query["timezone"] = "auto";
            query["forecast_days"] = "1";
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        async public Task<OpenMeteoTemperature> GetTomorrowTemperature(string latitude, string longitude) {
            string uri = GetUri(latitude, longitude);
            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode) {
                OpenMeteoTemperature? jResponse = await response.Content.ReadFromJsonAsync<OpenMeteoTemperature>();
                if (jResponse == null)
                    throw new HttpRequestException("No content found");
                return jResponse;
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
    }
}
