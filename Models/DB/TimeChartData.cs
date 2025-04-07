using Microsoft.EntityFrameworkCore;

namespace PredikceVytěžováníFVE.Models.DB {
    [PrimaryKey(nameof(TimeStamp))]
    public class TimeChartData<T> {
        public DateTime TimeStamp { get; set; }
        public T? Value { get; set; }

    }
}
