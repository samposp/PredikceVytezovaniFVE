using Microsoft.AspNetCore.SignalR;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;

namespace PredikceVytěžováníFVE.Services;

public class MqttDataService(FVEDbContext db, ILogger<MqttDataService> logger)
{
    public async Task<bool> SaveMqttData(MqttData data)
    {
        if (data.Date == null || data.Time == null)
        {
            logger.LogWarning("Date or Time is null. Cannot save MQTT data.");
            return false;
        }
        bool entryExists = db.MqttData.Any(x => x.Date!.Equals(data.Date) && x.Time!.Equals(data.Time));

        if (entryExists)
        {
            logger.LogInformation($"Entry for {data.Date} {data.Time} already exists. Skipping save.");
            return false;

        }
        logger.LogInformation($"Saving mqttMessage to DB");
        db.Add(data);
        await db.SaveChangesAsync();
        return true;
    }

    public List<MqttData> GetMqttDataByDate(DateTime date)
    {
        string dateString = date.ToString("dd.MM.yyyy");

        List<MqttData> mqttData = [.. db.MqttData.AsEnumerable().Where(x => x.Date!.Equals(dateString, StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => x.Time)];

        return mqttData.Select(x =>
        {
            x.DateTime = ConverterHelper.ToDateTime(x.Date, x.Time);
            return x;
        }).ToList();
    }
}
