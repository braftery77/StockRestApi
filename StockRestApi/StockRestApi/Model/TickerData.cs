
using System;

namespace StockRestApi.Model;

public class TickerData : IComparable<TickerData>, IEquatable<TickerData>
{
    public decimal Open { get; set; } = decimal.One;
    public decimal Close { get; set; } = decimal.One;
    public string Symbol { get; set; } = string.Empty;
    public DateTime PriceDate { get; set; } = DateTime.MinValue;
    public decimal DailyReturn => ((Close - Open) / Open) * 100;

    public int CompareTo(TickerData other)
    {
        return DateTime.Compare(PriceDate.Date, other.PriceDate.Date);
    }

    public bool Equals(TickerData other)
    {
        return PriceDate.Date.Equals(other.PriceDate.Date) && Symbol.Equals(other.Symbol, StringComparison.OrdinalIgnoreCase);
    }
}
