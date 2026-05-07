
namespace PredikceVytěžováníFVE.Models {
    public class Daily {
        public List<string> time { get; set; } = new();
        public List<double> temperature_2m_max { get; set; } = new();
        public List<double> temperature_2m_min { get; set; } = new();
    }

    public class Hourly
    {
        public List<string> time { get; set; } = new();
        public List<double> temperature_2m { get; set; } = new();
    }

    public class DailyUnits {
        public string time { get; set; } = string.Empty;
        public string temperature_2m_max { get; set; } = string.Empty;
        public string temperature_2m_min { get; set; } = string.Empty;
    }

    public class OpenMeteoTemperature {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double generationtime_ms { get; set; }
        public int utc_offset_seconds { get; set; }
        public string timezone { get; set; } = string.Empty;
        public string timezone_abbreviation { get; set; } = string.Empty;
        public double elevation { get; set; }
        public DailyUnits daily_units { get; set; } = new();
        public Daily daily { get; set; } = new();
        public Hourly hourly { get; set; } = new();
    }

}
