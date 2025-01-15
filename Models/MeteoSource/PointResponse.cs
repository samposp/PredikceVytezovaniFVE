namespace PredikceVytěžováníFVE.Models.MeteoSource
{

    public class PointResponse
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public float elevation { get; set; }
        public string timezone { get; set; }
        public string units { get; set; }
        public Current? current { get; set; }
        public Hourly hourly { get; set; }
        public Daily? daily { get; set; }
    }

    public class Current
    {
        public string icon { get; set; }
        public int icon_num { get; set; }
        public string summary { get; set; }
        public int temperature { get; set; }
        public Wind wind { get; set; }
        public Precipitation precipitation { get; set; }
        public int cloud_cover { get; set; }
    }

    public class Wind
    {
        public int speed { get; set; }
        public int angle { get; set; }
        public string dir { get; set; }
    }

    public class Precipitation
    {
        public float total { get; set; }
        public string type { get; set; }
    }

    public class Hourly
    {
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public DateTime date { get; set; }
        public string weather { get; set; }
        public int icon { get; set; }
        public string summary { get; set; }
        public float temperature { get; set; }
        public Wind1 wind { get; set; }
        public Cloud_Cover cloud_cover { get; set; }
        public Precipitation1 precipitation { get; set; }
    }

    public class Wind1
    {
        public float speed { get; set; }
        public string dir { get; set; }
        public float angle { get; set; }
    }

    public class Cloud_Cover
    {
        public float total { get; set; }
    }

    public class Precipitation1
    {
        public float total { get; set; }
        public string type { get; set; }
    }

    public class Daily
    {
        public Datum1[] data { get; set; }
    }

    public class Datum1
    {
        public string day { get; set; }
        public string weather { get; set; }
        public int icon { get; set; }
        public string summary { get; set; }
        public All_Day all_day { get; set; }
        public string morning { get; set; }
        public string afternoon { get; set; }
        public string evening { get; set; }
    }

    public class All_Day
    {
        public string weather { get; set; }
        public int icon { get; set; }
        public float temperature { get; set; }
        public float temperature_min { get; set; }
        public float temperature_max { get; set; }
        public Wind2 wind { get; set; }
        public Cloud_Cover1 cloud_cover { get; set; }
        public Precipitation2 precipitation { get; set; }
    }

    public class Wind2
    {
        public float speed { get; set; }
        public string dir { get; set; }
        public float angle { get; set; }
    }

    public class Cloud_Cover1
    {
        public float total { get; set; }
    }

    public class Precipitation2
    {
        public float total { get; set; }
        public string type { get; set; }
    }

}
