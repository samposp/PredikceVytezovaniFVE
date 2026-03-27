using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models.DB;

[PrimaryKey(nameof(TimeStamp))]
public class ControlPredictionBo
{
    public DateTime TimeStamp { get; set; }
    public List<float>? Capacity { get; set; }
    public List<float>? Charge { get; set; }
    public float? PriceSum { get; set; }
    public List<int>? ChargeTimes { get; set; }
    public List<int>? DischargeTimes { get; set; }
    public List<int>? ChargeToCapacities { get; set; }
}
