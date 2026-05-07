using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PredikceVytěžováníFVE.Backtest;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.Settings;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.BackTest;

public class BatteryBacktestService(ILogger<BatteryBacktestService> logger, SpotSoapService soapClient, ConcumptionPredictionService consumptionService, BatteryMilpOptimizationService batteryOptimizationService, PVForecastService oldFveService, FVEDbContext db)
{
        public async Task BackTest()
    {
        DateTime start = new DateTime(2026, 1, 20);
        DateTime end = new DateTime(2026, 3, 30);
        string resultFile = "backtestResult.csv";
        List<HistoricalDayInput> input = new();
        for (DateTime date = start; date <= end; date = date.AddDays(1))
        {
            var spotData = await soapClient.GetSoapData(date) ?? [];
            var fvePrediction =  oldFveService.GetPrediction(date).ToList();
            var consumptionPrediction = await consumptionService.GetPrediction(date) ?? [];
            if (spotData.Count == 0 || fvePrediction.Count == 0 || consumptionPrediction.Count == 0)
            {
                logger.LogWarning("Missing data for date {date}, skipping backtest for this day", date.ToShortDateString());
                continue;
            }
            input.Add(new HistoricalDayInput
            {
                Date = DateOnly.FromDateTime(date),
                InitialBatteryPercent = 20,
                Spot = ConverterHelper.ToFloatList(spotData),
                Fve = ConverterHelper.ToFloatList(fvePrediction),
                Consumption = ConverterHelper.ToFloatList(consumptionPrediction)
            });
        }

        var result = await Run(input);
        BacktestCsvWriter.SaveDayResults(resultFile, result.Days);
    }

    public async Task<BacktestSummary> Run(IEnumerable<HistoricalDayInput> days)
    {
        var summary = new BacktestSummary();

        foreach (var day in days)
        {
            summary.TotalDays++;

            try
            {
                ValidateDay(day);
                float noBatteryCost = CalculateNoBatteryCost(
                    day.Fve,
                    day.Consumption,
                    day.Spot);
                var existing = await db.PredictedControlData.FirstOrDefaultAsync(x => x.TimeStamp == day.Date.ToDateTime(new TimeOnly()));
                if (existing != null)
                {
                    var res = new BacktestDayResult
                    {
                        Date = day.Date,
                        OptimizedCost = existing.PriceSum ?? 0,
                        NoBatteryCost = noBatteryCost,
                        SavingsVsNoBattery = noBatteryCost - existing.PriceSum ?? 0,

                        TotalGridBuy = existing.GridBuy!.Sum(),
                        TotalGridSell = existing.GridSell!.Sum(),
                        TotalGridCharge = existing.Charge!.Sum(),

                        ChargeHoursCount = existing.ChargeTimes!.Count,
                        DischargeHoursCount = existing.DischargeTimes!.Count,

                        StartBatteryPercent = day.InitialBatteryPercent,
                        EndBatteryPercent = existing.Capacity!.LastOrDefault(),

                        Status = "OK"
                    };
                    summary.Days.Add(res);
                    summary.SuccessfulDays++;
                    continue;
                }
                BatteryOptimizationResult optimized = batteryOptimizationService.Optimize(
                    day.InitialBatteryPercent,
                    day.Fve,
                    day.Consumption,
                    day.Spot,
                    day.Date.ToDateTime(new TimeOnly()));

                // SAVE TO DB
                var prediction = ConverterHelper.ToControlPredictionBo(day.Date.ToDateTime(new TimeOnly()), optimized);

                if (existing is null)
                {
                    await db.PredictedControlData.AddAsync(prediction);
                }
                else
                {
                    existing.Capacity = prediction.Capacity;
                    existing.TimeStamp = prediction.TimeStamp;
                    existing.PriceSum = prediction.PriceSum;
                    existing.Charge = prediction.Charge;
                    existing.ChargeToCapacities = prediction.ChargeToCapacities;
                    existing.ChargeTimes = prediction.ChargeTimes;
                    existing.DischargeTimes = prediction.DischargeTimes;
                    existing.GridBuy = prediction.GridBuy;
                    existing.GridSell = prediction.GridSell;
                }

                await db.SaveChangesAsync();


                float optimizedCost = optimized.TotalCost;


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
        fve = fve.Select(x => x / 1000).ToList();
        consumption = consumption.Select(x => x / 1000).ToList();
        float total = 0f;

        fve = DataFilterHelper.InterpolateHourlyToQuarterHourly(fve);
        consumption = DataFilterHelper.InterpolateHourlyToQuarterHourly(consumption);

        for (int t = 0; t < 96; t++)
        {
            float currentSpot = spot[t] * 0.25f;
            float net = consumption[t] - fve[t];

            if (net > 0)
            {
                total += net * currentSpot;
            }
            else
            {
                total -= Math.Abs(net) * currentSpot;
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
