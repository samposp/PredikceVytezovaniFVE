using System.Text.Json.Serialization;

namespace PredikceVytěžováníFVE.Models.Settings;

public class FveInfo
{
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public float? PeakPower { get; set; }
    public float? Declination { get; set; }
    public float? Azimuth { get; set; }
}
