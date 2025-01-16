using Microsoft.AspNetCore.DataProtection.KeyManagement;
using PredikceVytěžováníFVE.Models.MeteoSource;
using PredikceVytěžováníFVE.Models.OpenWeather;
using System.Web;

namespace PredikceVytěžováníFVE.Services
{
    public class OpenWeatherService
    {

        const string APIKey = "REMOVED_API_KEY";
        const string host = "https://api.openweathermap.org/data/3.0/onecall";

        static private HttpClient client = new HttpClient();

        public async Task<OpenWeatherResponse> Call(string lat, string lon)
        {
            string uri = GetUrl(lat, lon);

            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                OpenWeatherResponse? weatherResponse = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
                if (weatherResponse == null)
                    throw new HttpRequestException("No content found");
                return weatherResponse;
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }

        private string GetUrl(string lat, string lon)
        {
            var uriBuilder = new UriBuilder(host);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["lat"] = lat;
            query["lon"] = lon;
            query["exclude"] = ""; // Available values: current,minutely,hourly,daily,alerts
            query["appid"] = APIKey;
            query["units"] = "metric";
            query["lang"] = "cz";
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }


    }
}
