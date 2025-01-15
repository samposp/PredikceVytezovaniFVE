namespace PredikceVytěžováníFVE.Models.MeteoSource
{
    public class PointQuery
    {
        public string? placeID { get; set; } // get from find_places_prefix | find_places 
        public string? lat { get; set; }    // format 12.3
        public string? lon { get; set; }    // format 12.3
        public string sections { get; set; } = "hourly";    // current/daily/hourly/all (separated by comma)
        public string timezone { get; set; } = "UTC";
        public string language { get; set; } = "en";    // only en for free tier
        public string units { get; set; } = "metric";
    }
}
