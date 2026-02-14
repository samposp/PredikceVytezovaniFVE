using Microsoft.Extensions.DependencyInjection;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using System;

namespace PredikceVytěžováníFVE.Services {
    // !!!This is the right one!!!
    public class PVForecastService(IServiceProvider serviceProvider) {

        private readonly string host = "https://www.pvforecast.cz/api/";

        // TODO: Read from config to be able to change it
        private readonly string _apiKey = "esvk7s";
        private string _latitude = "50.793";
        private string _longitude = "15.138";


        // http://www.pvforecast.cz/api/?key=esvk7s&lat=50.793&lon=15.138

        public IEnumerable<TimeValuePair> GetPrediction(DateTime date)
        {
            using var scope = serviceProvider.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();
            var storedData = db.PvForecastData.Where(p => p.TimeStamp.Date == date.Date);
            if (storedData.Any())
                return storedData.Select(d => new TimeValuePair(d.TimeStamp, d.Value)).ToList();
            return [];
        }

        public async Task<IEnumerable<TimeValuePair>> GetTommorowPrediciton() {

            DateTime tommorow = DateTime.Now.AddDays(1);
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();
            var storedData = db.PvForecastData.Where(p => p.TimeStamp.Date == tommorow.Date);
            if (storedData.Any())
                return storedData.Select(d => new TimeValuePair(d.TimeStamp, d.Value));

            Dictionary<string, string> parameters = new() {
                { "lat", _latitude },
                { "lon", _longitude },
                { "key", _apiKey },
                { "forecast", "pv" },
                { "type", "hour" },
                { "number", "24" },
                { "start", "tomorrow" },
                { "format", "json"}
            };
            string url = Helpers.UriHelper.CreateUrl([], parameters);

            using var client = new HttpClient();
            client.BaseAddress = new Uri(host);
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                List<List<object>>? pvforcastresponse = await response.Content.ReadFromJsonAsync<List<List<object>>>();
                if (pvforcastresponse == null)
                    throw new HttpRequestException("No content found");
                var responseValue = pvforcastresponse.Select(ToTimeValuePair);
                await db.AddRangeAsync(responseValue.Select(v => new PvForecastBo() { TimeStamp = v.DateTime, Value = v.Value }));
                await db.SaveChangesAsync();
                return responseValue;
            }
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }

        private TimeValuePair ToTimeValuePair(List<object> timeValue) {

            DateTime time = DateTime.Parse(timeValue[0].ToString() ?? throw new Exception("Can not parse date time"));
            decimal value = decimal.Parse(timeValue[1].ToString() ?? throw new Exception("Can not convert decimal value"));

            return new TimeValuePair(time, value);
        }
    }
}
