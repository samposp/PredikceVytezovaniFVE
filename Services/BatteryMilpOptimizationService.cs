using Google.OrTools.LinearSolver;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using System.Runtime.InteropServices;

namespace PredikceVytěžováníFVE.Services;

public class BatteryMilpOptimizationService(ConfigurationService config, ILogger<BatteryMilpOptimizationService> logger)
{
    public BatteryOptimizationResult Optimize(
        float initialBatteryPercent,
        List<float> fveHourly,
        List<float> consumptionHourly,
        List<float> spot,
        DateTime date)
    {
        if (fveHourly.Count != 24 || consumptionHourly.Count != 24)
            throw new Exception("Expected hourly data of length 24.");

        if (spot.Count != 96)
            throw new Exception("Expected spot price data of length 96");

        float capacity = config.Settings.Battery.Capacity
            ?? throw new Exception("Missing battery capacity in config");

        float minPercent = config.Settings.Battery.MinLevel
            ?? throw new Exception("Missing battery min level in config");

        float maxPercent = config.Settings.Battery.MaxLevel
            ?? throw new Exception("Missing battery max level in config");

        float chargeSpeed = config.Settings.Battery.ChargeSpeed
            ?? throw new Exception("Missing battery charge speed in config");

        float minSellPrice = config.Settings.Fve.MinSellPrice ?? 0.0f;

        double maxChargePower = chargeSpeed;

        double minCharge = capacity * minPercent / 100.0;
        double maxCharge = capacity * maxPercent / 100.0;
        double initialCharge = capacity * initialBatteryPercent / 100.0;

        List<float> fve = DataFilterHelper.InterpolateHourlyToQuarterHourly(fveHourly);
        List<float> consumption = DataFilterHelper.InterpolateHourlyToQuarterHourly(consumptionHourly);

        // convert all to kW
        fve = fve.Select(x => x / 1000).ToList();
        consumption = consumption.Select(x => x / 1000).ToList();


        if (initialCharge < minCharge || initialCharge > maxCharge)
            throw new Exception("Initial battery state is outside allowed bounds.");

        Solver solver = Solver.CreateSolver("SCIP");
        if (solver == null)
            throw new Exception("Cannot create SCIP solver.");

        int T = 96;
        double INF = double.PositiveInfinity;
        double dt = 0.25; // 15 minutes
        double eps = 1e-6;

        // ========= Variables =========

        // battery state 0..96
        Variable[] soc = new Variable[T + 1];

        Variable[] buy = new Variable[T];
        Variable[] sell = new Variable[T];
        Variable[] gridCharge = new Variable[T];
        Variable[] chargePv = new Variable[T];
        Variable[] discharge = new Variable[T];
        Variable[] curtailPv = new Variable[T];

        // binary modes
        Variable[] isGridCharge = new Variable[T];
        Variable[] isDischarge = new Variable[T];

        Variable[] startGridCharge = new Variable[T];
        Variable[] startDischarge = new Variable[T];

        for (int t = 0; t <= T; t++)
            soc[t] = solver.MakeNumVar(minCharge, maxCharge, $"soc_{t}");

        for (int t = 0; t < T; t++)
        {
            buy[t] = solver.MakeNumVar(0, INF, $"buy_{t}");
            sell[t] = solver.MakeNumVar(0, INF, $"sell_{t}");

            gridCharge[t] = solver.MakeNumVar(0, maxChargePower, $"gridCharge_{t}");
            chargePv[t] = solver.MakeNumVar(0, maxChargePower, $"chargePv_{t}");
            discharge[t] = solver.MakeNumVar(0, INF, $"discharge_{t}");
            curtailPv[t] = solver.MakeNumVar(0, INF, $"curtailPv_{t}");

            isGridCharge[t] = solver.MakeBoolVar($"isGridCharge_{t}");
            isDischarge[t] = solver.MakeBoolVar($"isDischarge_{t}");

            startGridCharge[t] = solver.MakeBoolVar($"startGridCharge_{t}");
            startDischarge[t] = solver.MakeBoolVar($"startDischarge_{t}");
        }

        // ========= Initial =========

        solver.Add(soc[0] == initialCharge);

        // ========= Constraints =========

        for (int t = 0; t < T; t++)
        {
            double pv = fve[t];
            double cons = consumption[t];

            double surplus = Math.Max(pv - cons, 0.0);
            double deficit = Math.Max(cons - pv, 0.0);

            // Cannot charge from grid and PV at the same time
            solver.Add(chargePv[t] <= maxChargePower * (1 - isGridCharge[t]));

            // Cannot grid-charge and discharge at the same time
            solver.Add(isGridCharge[t] + isDischarge[t] <= 1);

            // Grid charging only in grid-charge mode
            solver.Add(gridCharge[t] == maxChargePower * isGridCharge[t]);

            // PV charging can only use actual PV surplus
            solver.Add(chargePv[t] <= surplus);

            // No PV charging while discharging
            solver.Add(chargePv[t] <= maxChargePower * (1 - isDischarge[t]));

            solver.Add(curtailPv[t] <= surplus);
            if (spot[t] < minSellPrice)
            {
                solver.Add(sell[t] == 0);
            }

            // Discharge is all-or-nothing:
            // if isDischarge = 1 -> discharge exactly deficit
            // if isDischarge = 0 -> discharge = 0
            solver.Add(discharge[t] == deficit * isDischarge[t]);

            // don't overcharge battery within one step
            solver.Add(gridCharge[t] + chargePv[t] <= (maxCharge - soc[t]) / dt + eps);

            // don't overdischarge battery within one step
            solver.Add(discharge[t] <= (soc[t] - minCharge) / dt + eps);

            // grid balance:
            // pv + buy + discharge = cons + chargePv + gridCharge + sell
            solver.Add(
                pv + buy[t] + discharge[t]
                ==
                cons + chargePv[t] + gridCharge[t] + sell[t] + curtailPv[t]
            );

            // SOC update (no efficiency losses yet)
            solver.Add(
                soc[t + 1] ==
                soc[t] + dt * (chargePv[t] + gridCharge[t] - discharge[t])
            );

            // mode starts
            if (t == 0)
            {
                solver.Add(startGridCharge[t] >= isGridCharge[t]);
                solver.Add(startDischarge[t] >= isDischarge[t]);
            }
            else
            {
                solver.Add(startGridCharge[t] >= isGridCharge[t] - isGridCharge[t - 1]);
                solver.Add(startDischarge[t] >= isDischarge[t] - isDischarge[t - 1]);
            }

        }

        // Limit number of grid charging sessions per day
        solver.Add(startGridCharge.Sum() <= 2);

        // Limit number of discharge sessions per day
        solver.Add(startDischarge.Sum() <= 2);

        // ========= Objective =========

        double switchingPenalty = 0.01;
        //double chargeOutsideWindowPenalty = 0.03;
        //double dischargeOutsideWindowPenalty = 0.03;

        Objective objective = solver.Objective();

        for (int t = 0; t < T; t++)
        {
            double price = spot[t];

            // buy from grid
            objective.SetCoefficient(buy[t], price * dt);

            // sell to grid
            objective.SetCoefficient(sell[t], -price * dt);

            objective.SetCoefficient(curtailPv[t], 0.0);

            // optional battery wear penalty
            // objective.SetCoefficient(chargePv[t], batteryUsagePenalty);
            // objective.SetCoefficient(discharge[t], batteryUsagePenalty);

            // switching penalties
            objective.SetCoefficient(
                startGridCharge[t],
                objective.GetCoefficient(startGridCharge[t]) + switchingPenalty);

            // penalize starting a discharge block
            objective.SetCoefficient(
                startDischarge[t],
                objective.GetCoefficient(startDischarge[t]) + switchingPenalty);

            // FOR CHARGING IN SET TIMES - NOT NEEDED
            //int hour = t / 4;
            //bool preferredCharge =
            //    (hour >= 0 && hour < 5) ||
            //    (hour >= 10 && hour < 15);

            //bool preferredDischarge =
            //    (hour >= 5 && hour < 10) ||
            //    (hour >= 16 && hour < 23);

            //if (!preferredCharge)
            //{
            //    objective.SetCoefficient(
            //        isGridCharge[t],
            //        objective.GetCoefficient(isGridCharge[t]) + chargeOutsideWindowPenalty);
            //}

            //if (!preferredDischarge)
            //{
            //    objective.SetCoefficient(
            //        isDischarge[t],
            //        objective.GetCoefficient(isDischarge[t]) + dischargeOutsideWindowPenalty);
            //}
        }

        objective.SetMinimization();

        // ========= Solve =========

        var status = solver.Solve();
        if (status != Solver.ResultStatus.OPTIMAL &&
            status != Solver.ResultStatus.FEASIBLE)
        {
            throw new Exception($"MILP solver failed: {status}");
        }

        // ========= Result =========

        var result = new BatteryOptimizationResult();
        float buyCummulative = 0;
        float sellCummulative = 0;
        for (int t = 0; t < T; t++)
        {
            float spotPrice = (float)(spot[t] * dt);
            float socNowPercent = (float)(soc[t].SolutionValue() * 100.0 / capacity);
            float gCharge = (float)gridCharge[t].SolutionValue();
            float pCharge = (float)chargePv[t].SolutionValue();
            float dis = (float)discharge[t].SolutionValue();
            float delta = gCharge + pCharge - dis;
            float startCharge = (float)startGridCharge[t].SolutionValue();
            float startDis = (float)startDischarge[t].SolutionValue();

            result.BatteryCapacity.Add(socNowPercent);
            result.GridCharge.Add(gCharge);
            result.PvCharge.Add(pCharge);
            result.Discharge.Add(dis);
            result.BatteryDelta.Add(delta);

            var buyVal = (float)buy[t].SolutionValue() * spotPrice;
            var sellVal = (float)sell[t].SolutionValue() * spotPrice;
            if (buyVal < 0)
            {
                sellVal -= buyVal;
                buyVal = 0;
            }
            buyCummulative += buyVal;
            sellCummulative += sellVal;
            result.GridBuy.Add(buyCummulative);
            result.GridSell.Add(sellCummulative);

            if (startCharge > 0.001f)
                result.ChargeHours.Add(IndexToTime(date, t));

            if (startDis > 0.001f)
                result.DischargeHours.Add(IndexToTime(date, t));
        }

        result.TotalCost = (float)objective.Value();

        result.ChargeToCapacity = GetChargeTargets(result.BatteryCapacity, result.GridCharge);

        return result;
    }

    private static DateTime IndexToTime(DateTime date, int index)
    {
        return date.Date.Add(TimeSpan.FromMinutes(15 * index));
    }

    private static List<int> GetChargeTargets(List<float> batteryCapacityPercent, List<float> gridCharge)
    {
        var targets = new List<int>();

        if (batteryCapacityPercent.Count == 0 || gridCharge.Count == 0)
            return targets;

        bool inChargeBlock = false;
        float maxCapacityInBlock = 0f;

        for (int i = 0; i < gridCharge.Count; i++)
        {
            bool isChargingFromGrid = gridCharge[i] > 0.001f;

            if (isChargingFromGrid)
            {
                if (!inChargeBlock)
                {
                    inChargeBlock = true;
                    maxCapacityInBlock = batteryCapacityPercent[i];
                }

                maxCapacityInBlock = Math.Max(maxCapacityInBlock, batteryCapacityPercent[i]);
            }
            else if (inChargeBlock)
            {
                // include the SOC right after the last charging interval if available
                if (i < batteryCapacityPercent.Count)
                    maxCapacityInBlock = Math.Max(maxCapacityInBlock, batteryCapacityPercent[i]);

                targets.Add((int)Math.Round(maxCapacityInBlock));
                inChargeBlock = false;
            }
        }

        // if charging continues until the end of the horizon
        if (inChargeBlock)
        {
            maxCapacityInBlock = Math.Max(
                maxCapacityInBlock,
                batteryCapacityPercent[^1]);

            targets.Add((int)Math.Round(maxCapacityInBlock));
        }

        return targets;
    }
}