namespace PredikceVytěžováníFVE.Models.Settings;

public class ApplicationSettings
{
    public FveInfo Fve { get; set; } = new();
    public ApiInfo Api { get; set; } = new();
}

