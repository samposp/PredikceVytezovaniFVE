using PredikceVytěžováníFVE.Models;

namespace PredikceVytěžováníFVE.Helpers;

public static class DataFilterHelper
{
    public static List<MqttData> FilterDateTime(List<MqttData> inputList, TimeSpan? timeStep = null)
    {
        timeStep ??= new TimeSpan(0, 30, 0);

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

}

public interface HasTimeStamp
{
    DateTime? DateTime { get; set; }
}
