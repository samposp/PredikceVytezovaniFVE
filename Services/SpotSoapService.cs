using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using PublicOTEService;

namespace PredikceVytěžováníFVE.Services;

public class SpotSoapService(IServiceProvider serviceProvider, ILogger<SpotSoapService> logger)
{
    private readonly PublicDataServiceSoapClient oteClient = new();
    public async Task<List<TimeValuePair>?> GetSoapData(DateTime date)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();
        var dbData = db.SpotData.Where(db => db.DateTime.Date == date.Date).ToList();
        if (dbData != null && dbData.Count >= 24)
        {
            logger.LogInformation("Spot data retrieved from DB");
            return [.. dbData.Select(ConverterHelper.ToTimeValuePair).Select(x => new TimeValuePair(x.DateTime, x.Value))];
        }
        GetDamPricePeriodEResponse damPrice = await oteClient.GetDamPricePeriodEAsync(date, date,GetDamPricePeriodEPeriodResolution.PT15M, 1, 24*4);
        if (damPrice.Result.Length == 0)
        {
            logger.LogError("Spot data retrieval from SOAP failed");
            return null;
        }
        logger.LogInformation("Spot data retrieved from SOAP");

        var conversion = scope.ServiceProvider.GetRequiredService<EcbCurrencyConversionService>();
        var conversionRate = await conversion.GetCurrentEurToCzkRateAsync();
        var SpotData = damPrice.Result.Select(ConverterHelper.ToSpotBo).Select(x =>
        {
            return new SpotBo
            {
                DateTime = x.DateTime,
                Value = (conversionRate * x.Value) / 1000 // Convert from MWh to kWh
            };
        });
        await db.SpotData.AddRangeAsync(SpotData);
        await db.SaveChangesAsync();
        return [.. SpotData.Select(ConverterHelper.ToTimeValuePair).Select(x => new TimeValuePair(x.DateTime, x.Value))];
    }

    public async Task<List<TimeValuePair>?> GetHourlyAverageSoapData(DateTime date)
    {
        var quarterHourData = await GetSoapData(date);
        return quarterHourData?.GroupBy(q => q.DateTime.Hour)
            .Select(g => new TimeValuePair
            {
                DateTime = new DateTime(date.Year, date.Month, date.Day, g.Key, 0, 0),
                Value = g.Average(q => q.Value)
            }).ToList();
    } 
}
