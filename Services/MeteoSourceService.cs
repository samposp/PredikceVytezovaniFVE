using PredikceVytěžováníFVE.Models.MeteoSource;
using System.Web;

namespace PredikceVytěžováníFVE.Services
{
    public class MeteoSourceService
    {
        const string apiKey = "z7od15t9h6kfvgk0iaig59427rl45b02h6iv3ahl";

        const string host = "https://www.meteosource.com/api/v1/free/point";

        static private HttpClient client = new HttpClient();

        private string GetPointUri(PointQuery queryModel)
        {
            var uriBuilder = new UriBuilder(host);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["place_id"] = queryModel.placeID;
            query["lat"] = queryModel.lat;
            query["lon"] = queryModel.lon;
            query["sections"] = queryModel.sections;
            query["timezone"] = queryModel.timezone;
            query["language"] = queryModel.language;
            query["units"] = queryModel.units;
            query["key"] = apiKey;
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        async public Task<PointResponse> GetPoint(PointQuery query)
        {
            string uri = GetPointUri(query);
            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                PointResponse? pointResponse = await response.Content.ReadFromJsonAsync<PointResponse>();
                if (pointResponse == null)
                    throw new HttpRequestException("No content found");
                return pointResponse;
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
    }
}
