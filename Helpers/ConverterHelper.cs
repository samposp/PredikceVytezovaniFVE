using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using PredikceVytěžováníFVE.Services;
using PublicOTEService;

namespace PredikceVytěžováníFVE.Helpers;
public class ConverterHelper
{
    public static FVEData ToFVEData(MqttData message)
    {
        return new()
        {
            BatteryPercentage = message.SoC,
            BatteryOutput = message.P_Batt,
            PVOutput = message.P_PV,
            Consumption = (float?)message.Consumption,
            Grid = (float?)message.P_GRID,

            PVEnergyCumulative = message.PVenergy,
            OutputCumulative = message.Output,
            InputCumulative = message.Consumption,
            Timestamp = ToDateTime(message.Date, message.Time)
        };
    }
    public static TimeValuePair ToTimeValuePair(SpotBo spotBo)
    {
        return new TimeValuePair(spotBo.DateTime, spotBo.Value);
    }

    public static TimeValuePair ToTimeValuePair(ConsumptionForecastBo spotBo)
    {
        return new TimeValuePair(spotBo.TimeStamp, spotBo.Forecast ?? 0);
    }
    public static SpotBo ToSpotBo(GetDamPricePeriodEResponseItem spotPrice)
    {
        DateTime dateTime = spotPrice.Date;
        dateTime = dateTime.AddMinutes((spotPrice.PeriodIndex - 1) * 15);
        return new SpotBo
        {
            DateTime = dateTime,
            Value = (decimal)spotPrice.Price
        };
    }

    public static TimeValuePair ToTimeValuePair(WeatherForecastBo weatherBo)
    {
        return new TimeValuePair(weatherBo.TimeStamp, weatherBo.Temperature);
    }

    public static WeatherForecastBo ToWeatherForecastBo(TimeValuePair data)
    {
        return new WeatherForecastBo
        {
            TimeStamp = data.DateTime,
            Temperature = data.Value
        };
    }

    public static DateTime ToDateTime(string? date, string? time)
    {
        string format = "dd.MM.yyyy|HH:mm:ss";
        string dateTime = $"{date}|{time}";

        return DateTime.ParseExact(dateTime, format, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static List<float> FromCummulative(List<float> list)
    {
        float lastVal = 0;
        List<float> newList = [];
        foreach (float val in list)
        {
            newList.Add(val - lastVal);
            lastVal = val;
        }
        return newList;
    }

    public static List<float> ToFloatList(List<TimeValuePair> list)
    {
        return list.Select(x => (float)x.Value).ToList();
    }

    public static List<TimeValuePair> ToHourlyList(List<float>? list, DateTime? date = null)
    {
        if (list == null || list.Count == 0)
            return [];
        var dateTime = date ?? DateTime.Now;
        var hourList = DataFilterHelper.GetHourlyDateTimes(dateTime);
        if (list.Count != 24)
            throw new Exception("Input list not in hourly format!");

        List<TimeValuePair> newList = [];
        for (int i = 0; i < hourList.Count; i++)
        {
            newList.Add(new(hourList[i], (decimal)list[i]));
        }
        return newList;
    }

    public static List<TimeValuePair> ToQuarterHourlyList(List<float>? list, DateTime? date = null)
    {
        if (list == null)
            return [];
        var timespan = new TimeSpan(0, 15, 0);
        var dateTime = date ?? DateTime.Now;
        DateTime timeStamp = dateTime.Date;

        if (list.Count != 96)
            throw new Exception("Input list not in hourly format!");

        List<TimeValuePair> newList = [];
        for (int i = 0; i < list.Count; i++)
        {
            newList.Add(new(timeStamp, (decimal)list[i]));
            timeStamp = timeStamp.Add(timespan);
        }
        return newList;
    }

    //public static ControlPredictionBo ToControlPredictionBo(PredictedData data, DateTime date)
    //{
    //    return new()
    //    {
    //        TimeStamp = date.Date,
    //        Capacity = data.Capacity,
    //        Charge = data.Charge,
    //        ChargeTimes = data.ChargeTime,
    //        ChargeToCapacities = data.ChargeToCapacity,
    //        DischargeTimes = data.DischargeTime,
    //        PriceSum = data.PriceSum
    //    };
    //}

    //public static PredictedData ToPredictedData(ControlPredictionBo bo)
    //{
    //    return new(bo.Capacity!, bo.Charge!, bo.PriceSum ?? throw new Exception("Missing sum"), bo.ChargeTimes!, bo.DischargeTimes!, bo.ChargeToCapacities!);
    //}

    public static ControlPredictionBo ToControlPredictionBo(DateTime timeStamp, BatteryOptimizationResult source)
    {
        return new ControlPredictionBo
        {
            TimeStamp = timeStamp.Date,
            Capacity = source.BatteryCapacity?.ToList(),
            Charge = source.BatteryDelta?.ToList(),
            PriceSum = source.TotalCost,
            ChargeTimes = source.ChargeHours?.ToList(),
            DischargeTimes = source.DischargeHours?.ToList(),
            ChargeToCapacities = source.ChargeToCapacity?.ToList(),
            GridBuy = source.GridBuy?.ToList(),
            GridSell = source.GridSell?.ToList()
        };
    }
}
