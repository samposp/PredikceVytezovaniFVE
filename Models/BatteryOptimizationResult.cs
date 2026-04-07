namespace PredikceVytěžováníFVE.Models;

public class BatteryOptimizationResult
{
    public List<float> BatteryCapacity { get; set; } = [];
    public List<float> BatteryDelta { get; set; } = [];          // + nabíjení, - vybíjení
    public List<float> GridBuy { get; set; } = [];
    public List<float> GridSell { get; set; } = [];
    public List<float> GridCharge { get; set; } = [];            // nabíjení ze sítě
    public List<float> PvCharge { get; set; } = [];              // nabíjení z FVE
    public List<float> Discharge { get; set; } = [];             // vybíjení do domu

    public float TotalCost { get; set; }

    public List<DateTime> ChargeHours { get; set; } = [];
    public List<DateTime> DischargeHours { get; set; } = [];
    public List<int> ChargeToCapacity { get; set; } = [];        // target SOC v %

    public float NightTargetPercent { get; set; }
    public float NoonTargetPercent { get; set; }
}