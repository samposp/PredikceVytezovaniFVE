using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models.DB;


[PrimaryKey(nameof(TimeStamp))]
public class WeatherForecastBo
{
    public DateTime TimeStamp { get; set; }
    public decimal Temperature { get; set; }
}