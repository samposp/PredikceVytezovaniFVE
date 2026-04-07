namespace PredikceVytěžováníFVE.BackTest;

public class HistoricalDayInput
{
    public DateOnly Date { get; set; }
    public float InitialBatteryPercent { get; set; }

    public List<float> Fve { get; set; } = new();
    public List<float> Consumption { get; set; } = new();
    public List<float> Spot { get; set; } = new();
}

public class BacktestDayResult
{
    public DateOnly Date { get; set; }

    public float OptimizedCost { get; set; }
    public float NoBatteryCost { get; set; }
    public float SavingsVsNoBattery { get; set; }

    public float TotalGridBuy { get; set; }
    public float TotalGridSell { get; set; }
    public float TotalGridCharge { get; set; }
    public float TotalPvCharge { get; set; }
    public float TotalDischarge { get; set; }

    public int ChargeHoursCount { get; set; }
    public int DischargeHoursCount { get; set; }

    public float StartBatteryPercent { get; set; }
    public float EndBatteryPercent { get; set; }

    public string Status { get; set; } = "OK";
    public string? Error { get; set; }
}

public class BacktestSummary
{
    public int TotalDays { get; set; }
    public int SuccessfulDays { get; set; }
    public int FailedDays { get; set; }

    public float TotalOptimizedCost { get; set; }
    public float TotalNoBatteryCost { get; set; }
    public float TotalSavingsVsNoBattery { get; set; }

    public float AverageOptimizedCost { get; set; }
    public float AverageNoBatteryCost { get; set; }
    public float AverageSavingsVsNoBattery { get; set; }

    public float BestSavingsDay { get; set; }
    public float WorstSavingsDay { get; set; }

    public List<BacktestDayResult> Days { get; set; } = new();
}