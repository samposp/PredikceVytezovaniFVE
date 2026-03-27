using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Helpers;

namespace PredikceVytěžováníFVE.Models;
[PrimaryKey(nameof(Date), nameof(Time))]
public class MqttData : HasTimeStamp
{
    public int? SoC { get; set; }        // procento nabití baterie
    public int? P_PV { get; set; }       // výkon stringu
    public int? P_Inv { get; set; }      // výkon měniče
    public int? P_OnGr { get; set; }     //
    public int? P_OffGr { get; set; }    //
    public int? P_Batt { get; set; }     // z baterie do domu nebo její nabíjení podle znaménka
    public double? PVenergy { get; set; }    //
    public double? Charge { get; set; }      // množství energie do baterie od půlnoci
    public double? Discharge { get; set; }   // množství energie z baterie od půlnoci
    public double? Feed { get; set; }        // množství energie ze sítě od půlnoci
    public double? Consumption { get; set; } // spotřeba domu od půlnoci
    public double? Output { get; set; }      // přetoky do sítě od půlnoci
    public double? Input { get; set; }
    public double? Load { get; set; }
    public string? Date { get; set; }        // časová značka
    public string? Time { get; set; }
    public DateTime? DateTime { get; set; }

    public int? GRID_LIMIT { get; set; }
    public double? PRICE_CZK { get; set; }
    public int? P_HOME { get; set; }
    public int? P_HOME_L1 { get; set; }
    public int? P_HOME_L2 { get; set; }
    public int? P_HOME_L3 { get; set; }
    public double? P_EPS { get; set; }
    public int? P_GRID { get; set; }
    public int? P_GRID_L1 { get; set; }
    public int? P_GRID_L2 { get; set; }
    public int? P_GRID_L3 { get; set; }
    public int? P_BAT { get; set; }
    public double? PVenergy_T { get; set; }
    public double? ToBAT { get; set; }
    public double? ToBAT_T { get; set; }
    public double? FromBAT { get; set; }
    public double? FromBAT_T { get; set; }
    public double? SELL { get; set; }
    public double? SELL_T { get; set; }
    public double? BUY { get; set; }
    public double? BUY_T { get; set; }


    public string? FullMessage { get; set; }

    public static MqttData operator +(MqttData left, MqttData right)
    {
        if (left is null && right is null) return new MqttData();
        if (left is null) return right;
        if (right is null) return left;

        return new MqttData
        {
            SoC = Add(left.SoC, right.SoC),
            P_PV = Add(left.P_PV, right.P_PV),
            P_Inv = Add(left.P_Inv, right.P_Inv),
            P_OnGr = Add(left.P_OnGr, right.P_OnGr),
            P_OffGr = Add(left.P_OffGr, right.P_OffGr),
            P_Batt = Add(left.P_Batt, right.P_Batt),
            PVenergy = Add(left.PVenergy, right.PVenergy),
            Charge = Add(left.Charge, right.Charge),
            Discharge = Add(left.Discharge, right.Discharge),
            Feed = Add(left.Feed, right.Feed),
            Consumption = Add(left.Consumption, right.Consumption),
            Output = Add(left.Output, right.Output),
            Input = Add(left.Input, right.Input),
            Load = Add(left.Load, right.Load),

            // usually not summed; keep left if present, otherwise right
            Date = left.Date ?? right.Date,
            Time = left.Time ?? right.Time,
            DateTime = left.DateTime ?? right.DateTime,

            GRID_LIMIT = Add(left.GRID_LIMIT, right.GRID_LIMIT),
            PRICE_CZK = Add(left.PRICE_CZK, right.PRICE_CZK),
            P_HOME = Add(left.P_HOME, right.P_HOME),
            P_HOME_L1 = Add(left.P_HOME_L1, right.P_HOME_L1),
            P_HOME_L2 = Add(left.P_HOME_L2, right.P_HOME_L2),
            P_HOME_L3 = Add(left.P_HOME_L3, right.P_HOME_L3),
            P_EPS = Add(left.P_EPS, right.P_EPS),
            P_GRID = Add(left.P_GRID, right.P_GRID),
            P_GRID_L1 = Add(left.P_GRID_L1, right.P_GRID_L1),
            P_GRID_L2 = Add(left.P_GRID_L2, right.P_GRID_L2),
            P_GRID_L3 = Add(left.P_GRID_L3, right.P_GRID_L3),
            P_BAT = Add(left.P_BAT, right.P_BAT),
            PVenergy_T = Add(left.PVenergy_T, right.PVenergy_T),
            ToBAT = Add(left.ToBAT, right.ToBAT),
            ToBAT_T = Add(left.ToBAT_T, right.ToBAT_T),
            FromBAT = Add(left.FromBAT, right.FromBAT),
            FromBAT_T = Add(left.FromBAT_T, right.FromBAT_T),
            SELL = Add(left.SELL, right.SELL),
            SELL_T = Add(left.SELL_T, right.SELL_T),
            BUY = Add(left.BUY, right.BUY),
            BUY_T = Add(left.BUY_T, right.BUY_T),

            FullMessage = left.FullMessage ?? right.FullMessage
        };
    }

    private static int? Add(int? a, int? b)
    {
        if (a.HasValue && b.HasValue) return a.Value + b.Value;
        return a ?? b;
    }

    private static double? Add(double? a, double? b)
    {
        if (a.HasValue && b.HasValue) return a.Value + b.Value;
        return a ?? b;
    }

    public static MqttData operator /(MqttData data, double divisor)
    {
        if (data is null)
            return new MqttData();

        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide MqttData by zero.");

        return new MqttData
        {
            SoC = DivideInt(data.SoC, divisor),
            P_PV = DivideInt(data.P_PV, divisor),
            P_Inv = DivideInt(data.P_Inv, divisor),
            P_OnGr = DivideInt(data.P_OnGr, divisor),
            P_OffGr = DivideInt(data.P_OffGr, divisor),
            P_Batt = DivideInt(data.P_Batt, divisor),

            PVenergy = DivideDouble(data.PVenergy, divisor),
            Charge = DivideDouble(data.Charge, divisor),
            Discharge = DivideDouble(data.Discharge, divisor),
            Feed = DivideDouble(data.Feed, divisor),
            Consumption = DivideDouble(data.Consumption, divisor),
            Output = DivideDouble(data.Output, divisor),
            Input = DivideDouble(data.Input, divisor),
            Load = DivideDouble(data.Load, divisor),

            Date = data.Date,
            Time = data.Time,
            DateTime = data.DateTime,

            GRID_LIMIT = DivideInt(data.GRID_LIMIT, divisor),
            PRICE_CZK = DivideDouble(data.PRICE_CZK, divisor),
            P_HOME = DivideInt(data.P_HOME, divisor),
            P_HOME_L1 = DivideInt(data.P_HOME_L1, divisor),
            P_HOME_L2 = DivideInt(data.P_HOME_L2, divisor),
            P_HOME_L3 = DivideInt(data.P_HOME_L3, divisor),
            P_EPS = DivideDouble(data.P_EPS, divisor),
            P_GRID = DivideInt(data.P_GRID, divisor),
            P_GRID_L1 = DivideInt(data.P_GRID_L1, divisor),
            P_GRID_L2 = DivideInt(data.P_GRID_L2, divisor),
            P_GRID_L3 = DivideInt(data.P_GRID_L3, divisor),
            P_BAT = DivideInt(data.P_BAT, divisor),
            PVenergy_T = DivideDouble(data.PVenergy_T, divisor),
            ToBAT = DivideDouble(data.ToBAT, divisor),
            ToBAT_T = DivideDouble(data.ToBAT_T, divisor),
            FromBAT = DivideDouble(data.FromBAT, divisor),
            FromBAT_T = DivideDouble(data.FromBAT_T, divisor),
            SELL = DivideDouble(data.SELL, divisor),
            SELL_T = DivideDouble(data.SELL_T, divisor),
            BUY = DivideDouble(data.BUY, divisor),
            BUY_T = DivideDouble(data.BUY_T, divisor),

            FullMessage = data.FullMessage
        };
    }

    private static int? DivideInt(int? value, double divisor)
    {
        return value.HasValue ? (int?)Math.Round(value.Value / divisor) : null;
    }

    private static double? DivideDouble(double? value, double divisor)
    {
        return value.HasValue ? value.Value / divisor : null;
    }
}
