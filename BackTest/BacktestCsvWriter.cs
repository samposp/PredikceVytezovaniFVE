using PredikceVytěžováníFVE.BackTest;

namespace PredikceVytěžováníFVE.Backtest;

public static class BacktestCsvWriter
{
    public static void SaveDayResults(string path, List<BacktestDayResult> days)
    {
        using var writer = new StreamWriter(path);

        writer.WriteLine(
            "Date,Status,Error,OptimizedCost,NoBatteryCost,SavingsVsNoBattery," +
            "TotalGridBuy,TotalGridSell,TotalGridCharge,TotalPvCharge,TotalDischarge," +
            "ChargeHoursCount,DischargeHoursCount,StartBatteryPercent,EndBatteryPercent");

        foreach (var d in days.OrderBy(x => x.Date))
        {
            writer.WriteLine(string.Join(",",
                d.Date.ToString("yyyy-MM-dd"),
                Escape(d.Status),
                Escape(d.Error ?? ""),
                d.OptimizedCost.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.NoBatteryCost.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.SavingsVsNoBattery.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.TotalGridBuy.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.TotalGridSell.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.TotalGridCharge.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.TotalPvCharge.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.TotalDischarge.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.ChargeHoursCount.ToString(),
                d.DischargeHoursCount.ToString(),
                d.StartBatteryPercent.ToString(System.Globalization.CultureInfo.InvariantCulture),
                d.EndBatteryPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)
            ));
        }
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }
}
