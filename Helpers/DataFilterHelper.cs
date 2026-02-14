namespace PredikceVytěžováníFVE.Helpers;

public static class DataFilterHelper
{
    public static List<T> FilterDateTime<T>(List<T> inputList, TimeSpan? timeStep = null) where T : HasTimeStamp
    {
        timeStep ??= new TimeSpan(0, 5, 0);

        if (inputList.Count == 0)
            return [];

        List<T> outputList = [inputList[0]];
        DateTime lastTime = DateTime.MinValue;
        foreach (var item in inputList.ToList())
        {
            if (item.DateTime.HasValue)
            {
                var timeDifference = item.DateTime.Value - lastTime;
                if (timeDifference > timeStep)
                {
                    lastTime = item.DateTime.Value;
                    outputList.Add(item);
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
