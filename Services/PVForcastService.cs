using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Hosting;
using PredikceVytěžováníFVE.Models;
using System.Web;

namespace PredikceVytěžováníFVE.Services {

    public class PVForcastService {

        private readonly string host = "https://www.pvforecast.cz/api/";
        private readonly string apiKey = "huph8n";

        static private HttpClient client = new HttpClient();

        public PVForcastService()
        {
            client.BaseAddress = new Uri(host);
        }

        public async Task<IEnumerable<TimeValuePair>> GetPrediciton(string latitude, string longitude) {
            Dictionary<string, string> parameters = new() {
                { "lat", latitude },
                { "lon", longitude },
                { "key", apiKey },
                { "forecast", "pv" },
                { "type", "hour" },
                { "number", "24" },
                { "start", "tomorrow" },
                { "format", "json"}
            };
            string url = Helpers.UriHelper.CreateUrl(new List<string>(), parameters);

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                List<List<object>>? pvforcastresponse = await response.Content.ReadFromJsonAsync<List<List<object>>>();
                if (pvforcastresponse == null)
                    throw new HttpRequestException("No content found");
                return pvforcastresponse.Select(ToTimeValuePair);
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }

        private TimeValuePair ToTimeValuePair(List<object> timeValue) {

            DateTime time = DateTime.Parse((string)timeValue[0]);
            decimal value = (decimal)timeValue[1];

            return new TimeValuePair(time, value);
        }
    }
}
