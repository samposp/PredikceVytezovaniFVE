using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models.DB;

[PrimaryKey(nameof(TimeStamp))]
public class ControlPredictionBo
{
    public DateTime TimeStamp { get; set; }
    public List<float>? Capacity { get; set; }
    public List<float>? Charge { get; set; }
    public float? PriceSum { get; set; }
    public List<DateTime>? ChargeTimes { get; set; }
    public List<DateTime>? DischargeTimes { get; set; }
    public List<int>? ChargeToCapacities { get; set; }
    public List<float>? GridBuy { get; set; }
    public List<float>? GridSell { get; set; }
}
