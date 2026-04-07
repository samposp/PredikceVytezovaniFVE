using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.BackTest;

public class BatteryBacktestService(BatteryMilpOptimizationService optimizer)
{

    public BacktestSummary Run(IEnumerable<HistoricalDayInput> days)
    {
        var summary = new BacktestSummary();

        foreach (var day in days)
        {
            summary.TotalDays++;

            try
            {
                ValidateDay(day);

                BatteryOptimizationResult optimized = optimizer.Optimize(
                    day.InitialBatteryPercent,
                    day.Fve,
                    day.Consumption,
                    day.Spot,
                    day.Date.ToDateTime(new TimeOnly()));

                float optimizedCost = optimized.TotalCost;
                float noBatteryCost = CalculateNoBatteryCost(
                    day.Fve,
                    day.Consumption,
                    day.Spot);

                var result = new BacktestDayResult
                {
                    Date = day.Date,
                    OptimizedCost = optimizedCost,
                    NoBatteryCost = noBatteryCost,
                    SavingsVsNoBattery = noBatteryCost - optimizedCost,

                    TotalGridBuy = optimized.GridBuy.Sum(),
                    TotalGridSell = optimized.GridSell.Sum(),
                    TotalGridCharge = optimized.GridCharge.Sum(),
                    TotalPvCharge = optimized.PvCharge.Sum(),
                    TotalDischarge = optimized.Discharge.Sum(),

                    ChargeHoursCount = optimized.ChargeHours.Count,
                    DischargeHoursCount = optimized.DischargeHours.Count,

                    StartBatteryPercent = day.InitialBatteryPercent,
                    EndBatteryPercent = optimized.BatteryCapacity.LastOrDefault(),

                    Status = "OK"
                };

                summary.Days.Add(result);
                summary.SuccessfulDays++;
            }
            catch (Exception ex)
            {
                summary.Days.Add(new BacktestDayResult
                {
                    Date = day.Date,
                    StartBatteryPercent = day.InitialBatteryPercent,
                    Status = "FAILED",
                    Error = ex.Message
                });

                summary.FailedDays++;
            }
        }

        FinalizeSummary(summary);
        return summary;
    }

    private static void ValidateDay(HistoricalDayInput day)
    {
        if (day.Fve.Count != 24)
            throw new Exception($"Day {day.Date}: FVE must have 24 values.");

        if (day.Consumption.Count != 24)
            throw new Exception($"Day {day.Date}: Consumption must have 24 values.");

        if (day.Spot.Count != 96)
            throw new Exception($"Day {day.Date}: Spot must have 96 values.");
    }

    private static float CalculateNoBatteryCost(
        List<float> fve,
        List<float> consumption,
        List<float> spot)
    {
        // convert all to kW
        fve = fve.Select(x => x/1000).ToList();
        consumption = consumption.Select(x => x/1000).ToList();
        float total = 0f;

        for (int t = 0; t < 24; t++)
        {
            float net = consumption[t] - fve[t];

            if (net > 0)
            {
                total += net * spot[t];
            }
            else
            {
                total -= Math.Abs(net) * spot[t];
            }
        }

        return total;
    }

    private static void FinalizeSummary(BacktestSummary summary)
    {
        var okDays = summary.Days.Where(x => x.Status == "OK").ToList();

        if (okDays.Count == 0)
            return;

        summary.TotalOptimizedCost = okDays.Sum(x => x.OptimizedCost);
        summary.TotalNoBatteryCost = okDays.Sum(x => x.NoBatteryCost);
        summary.TotalSavingsVsNoBattery = okDays.Sum(x => x.SavingsVsNoBattery);

        summary.AverageOptimizedCost = summary.TotalOptimizedCost / okDays.Count;
        summary.AverageNoBatteryCost = summary.TotalNoBatteryCost / okDays.Count;
        summary.AverageSavingsVsNoBattery = summary.TotalSavingsVsNoBattery / okDays.Count;

        summary.BestSavingsDay = okDays.Max(x => x.SavingsVsNoBattery);
        summary.WorstSavingsDay = okDays.Min(x => x.SavingsVsNoBattery);
    }
}
