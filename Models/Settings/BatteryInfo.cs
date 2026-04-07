namespace PredikceVytěžováníFVE.Models.Settings;

public class BatteryInfo
{
    public int? MinLevel { get; set; }
    public int? MaxLevel { get; set; }
    public float? ChargeSpeed { get; set; } // kW per h
    public float? Capacity { get; set; } 
}
