using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models.DB;

[PrimaryKey(nameof(TimeStamp))]
public class PvForecastBo
{
    public DateTime TimeStamp { get; set; }
    public decimal Value { get; set; }

}
