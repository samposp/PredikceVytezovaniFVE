using PredikceVytěžováníFVE.Models.OpenWeather;
using System;
using System.Web;

namespace PredikceVytěžováníFVE.Services {
    public class SolcastService {
        private readonly string APIKey = "ihlo2c1-uZkdwcvDkZqP8oECCZ7xifeU";
        private readonly string host = "https://api.solcast.com.au/data/forecast/";

        static private HttpClient client = new HttpClient();


        /*
         FREE TEST LOCATIONS
        Location	            Latitude	Longitude	Advanced PV Power Resource ID
        Sydney Opera House	    -33.856784	151.215297	ba75-e17a-7374-95ed
        Grand Canyon	        36.099763	-112.112485	375f-eb3e-71c0-ef5e
        Stonehenge	            51.178882	-1.826215	1a57-6b1f-ec18-c5c8
        The Colosseum	        41.89021	12.492231	5f86-4c8f-2cb3-0215
        Giza Pyramid Complex	29.977296	31.132496	8d10-f530-af85-5cbb
        Taj Mahal	            27.175145	78.042142	b926-8fd2-ad3f-e4f5
        Fort Peck (SURFRAD)	    48.30783	-105.1017	3ae7-2456-492c-9aba
        Goodwin Creek (SURFRAD)	34.2547	    -89.8729	b787-cf17-e429-ef1d
         */
        public async Task Call() {
            Dictionary<string, string> parameters = new() {
                { "latitude", "-33.856784" },
                { "longitude", "151.215297" },
                { "hours", "24" },
                { "capacity", "1" },
                { "format", "json"}
            };
            string url = GetUrl("rooftop_pv_power", parameters);

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                string solcastResponse = await response.Content.ReadAsStringAsync();
                if (solcastResponse == null)
                    throw new HttpRequestException("No content found");
                return;
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }

        private string GetUrl(string path, Dictionary<string, string> queryParameters) {
            var uriBuilder = new UriBuilder(host + path);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var queryParam in queryParameters) {
                query[queryParam.Key] = queryParam.Value;
            }
            query["api_key"] = APIKey;
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }
    }
}
