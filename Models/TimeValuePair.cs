using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Helpers;

namespace PredikceVytěžováníFVE.Models;

[PrimaryKey(nameof(DateTime))]
public class TimeValuePair {
    public DateTime DateTime { get; set; }
    public decimal Value { get; set; }

    public TimeValuePair(DateTime time, decimal value) {
        DateTime = time;
        Value = value;
    }
    public TimeValuePair() { }
};
