using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
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
            return [.. dbData.Select(ConverterHelper.ToTimeValuePair)];
        }
        GetDamPricePeriodEResponse damPrice = await oteClient.GetDamPricePeriodEAsync(date, date,GetDamPricePeriodEPeriodResolution.PT15M, 1, 24*4);
        if (damPrice.Result.Length == 0)
        {
            logger.LogError("Spot data retrieval from SOAP failed");
            return null;
        }
        logger.LogInformation("Spot data retrieved from SOAP");
        var SpotData = damPrice.Result.Select(ConverterHelper.ToSpotBo);
        await db.SpotData.AddRangeAsync(SpotData);
        await db.SaveChangesAsync();
        return [.. SpotData.Select(ConverterHelper.ToTimeValuePair)];
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
