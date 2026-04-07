using PredikceVytěžováníFVE.Models;

namespace PredikceVytěžováníFVE.Helpers;

public static class DataFilterHelper
{
    public static List<MqttData> FilterDateTime(List<MqttData> inputList, TimeSpan? timeStep = null)
    {
        timeStep ??= new TimeSpan(0, 15, 0);

        if (inputList.Count == 0)
            return [];
        inputList.RemoveAt(0);
        if (inputList.Count == 0)
            return [];
        List<MqttData> outputList = [inputList[0]];
        DateTime lastTime = DateTime.MinValue;
        MqttData sumValues = new();
        int count = 0;
        foreach (var item in inputList.ToList())
        {
            count++;
            sumValues += item;
            if (item.DateTime.HasValue)
            {
                var timeDifference = item.DateTime.Value - lastTime;
                if (timeDifference > timeStep)
                {
                    lastTime = item.DateTime.Value;
                    var average = sumValues / count;
                    outputList.Add(item);
                    sumValues = new();
                    count = 0;
                }
            }
        }
        return outputList;
    }

    

    public static List<DateTime> GetHourlyDateTimes(DateTime date)
    {
        date = date.Date; // Set time to 00:00:00
        List<DateTime> dateTimes = [];
        for (int i = 0; i < 24; i++)
        {
            dateTimes.Add(date.AddHours(i));
        }
        return dateTimes;
    }

    public static List<float> InterpolateHourlyToQuarterHourly(List<float> hourly)
    {
        if (hourly.Count != 24)
            throw new Exception("Expected 24 hourly values.");

        var result = new List<float>(96);

        for (int h = 0; h < 24; h++)
        {
            float v0 = hourly[h];
            float v1 = (h < 23) ? hourly[h + 1] : hourly[h]; // hold last hour flat

            result.Add(v0);                           // :00
            result.Add(v0 + (v1 - v0) * 0.25f);      // :15
            result.Add(v0 + (v1 - v0) * 0.50f);      // :30
            result.Add(v0 + (v1 - v0) * 0.75f);      // :45
        }

        return result;
    }

    public static List<float> QuarterHourlyAddedToHourly(List<float> quarterHourly)
    {
        if (quarterHourly.Count != 96)
            throw new Exception("Expected 96 quarter hourly values.");

        var hourly = new List<float>(24);

        for (int i = 0; i < 96; i+=4)
        {
            var hourSum = quarterHourly[i] + quarterHourly[i + 1] + quarterHourly[i + 2] + quarterHourly[i + 3];
            hourly.Add(hourSum);
        }

        return hourly;
    }

}

public interface HasTimeStamp
{
    DateTime? DateTime { get; set; }
}
