using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models.DB;

[PrimaryKey(nameof(TimeStamp))]
public class HourlyBo
{
    public DateTime TimeStamp { get; set; }
    public decimal? P_HOME { get; set; }
    public decimal? Temperature { get; set; }
}
