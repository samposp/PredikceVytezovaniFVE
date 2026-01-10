using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using PredikceVytěžováníFVE.Models.Forecast;
using ServiceReference1;

namespace PredikceVytěžováníFVE.Services {
    public interface IScopedSchedulerService {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }

    public class ScopedSchedulerService : IScopedSchedulerService {
        private readonly ILogger<ScopedSchedulerService> _logger;
        private readonly FVEDbContext _database;
        private readonly ForecastService _forecastService = new();
        private readonly PublicDataServiceSoapClient _soapClient = new();


        public ScopedSchedulerService(ILogger<ScopedSchedulerService> logger, FVEDbContext database) {
            _logger = logger;
            _database = database;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await GetSpot();
            await GetForecast();
        }

        private async Task GetForecast() {
            string latitude = "50.79";
            string longitude = "15.145";
            string azimuth = "-15";
            string peakPower = "19.9";
            string declination = "35";
            WatthourResponse forecast = await _forecastService.GetWatthours(latitude, longitude, peakPower, declination, azimuth);
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
            List<TimeChartData<int>> forecastData = ToChartData(forecast).Where(x => x.TimeStamp == tomorrow).ToList();
            await _database.forecastData.AddRangeAsync(forecastData);
            await _database.SaveChangesAsync();
        }

        private async Task GetSpot() {
            DateTime tomorrow = DateTime.Now.AddDays(1);
            GetDamPriceEResponse damPrice = await _soapClient.GetDamPriceEAsync(tomorrow, tomorrow, 1, 24, false);
            List<TimeChartData<double>> spotData = ToChartData(damPrice);
            await _database.spotData.AddRangeAsync(spotData);
            await _database.SaveChangesAsync();
        }
        private List<TimeChartData<double>> ToChartData(GetDamPriceEResponse damPrice) {
            return damPrice.Result.Select(x => {
                return new TimeChartData<double>() {
                    TimeStamp = x.Date.AddHours(x.Hour),
                    Value = (double)x.Price
                };
            }).ToList();
        }

        private List<TimeChartData<int>> ToChartData(WatthourResponse wattHour) {
            return wattHour.Result.Data.Select(x => new TimeChartData<int>() {
                TimeStamp = x.Key,
                Value = x.Value
            }).ToList();
        }
    }
}