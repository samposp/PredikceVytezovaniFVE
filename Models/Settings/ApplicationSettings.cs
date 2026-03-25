namespace PredikceVytěžováníFVE.Models.Settings;

public class ApplicationSettings
{
    public FveInfo Fve { get; set; } = new();
    public ApiInfo Api { get; set; } = new();
    public ModelInfo Model { get; set; } = new();
    public BatteryInfo Battery { get; set; } = new();
}

