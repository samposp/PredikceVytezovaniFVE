using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models.DB;

[PrimaryKey(nameof(DateTime))]
public class SpotBo
{
    public DateTime DateTime { get; set; }
    public decimal Value { get; set; }
}
