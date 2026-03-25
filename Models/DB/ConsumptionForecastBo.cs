using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace PredikceVytěžováníFVE.Models.DB;

[PrimaryKey(nameof(TimeStamp))]
public class ConsumptionForecastBo
{
    [Column("index")]
    public DateTime TimeStamp { get; set; }

    [Column("forecast")]
    public decimal? Forecast { get; set; }
}
