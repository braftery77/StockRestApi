using StockRestApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockRestApi.Services;

public interface ITickerCache
{
    Task<List<TickerData>> GetOrAddAsync(string symbol, DateTime fromDate, DateTime toDate, Func<Task<List<TickerData>>> callback);
}

public class TickerCache : ITickerCache
{
    private static readonly Dictionary<string, List<DateTime>> _savedRanges = new Dictionary<string, List<DateTime>>();
    private static readonly Dictionary<string, List<TickerData>> _tickerDictionary = new Dictionary<string, List<TickerData>>();

    public async Task<List<TickerData>> GetOrAddAsync(string symbol, DateTime fromDate, DateTime toDate, Func<Task<List<TickerData>>> callback)
    {
        if (HasRangeCached(symbol, fromDate, toDate))
        {
            var cachedData = _tickerDictionary.GetValueOrDefault(symbol);
            var startIndex = cachedData.IndexOf(cachedData.Where(cd => cd.PriceDate == fromDate).First());
            var toIndex = cachedData.IndexOf(cachedData.Where(cd => cd.PriceDate == toDate).First());
            var range = cachedData.GetRange(startIndex, (toIndex - startIndex) + 1);
            range.Reverse();
            return range;
        }

        var callbackResult = await callback.Invoke();

        _ = Task.Run(() => SaveToCache(callbackResult, symbol));

        return callbackResult;
    }

    // Check to see if we have all data within a range for a given symbol
    public bool HasRangeCached(string symbol, DateTime fromDate, DateTime toDate)
    {
        var ranges = _savedRanges.GetValueOrDefault(symbol);
        if (ranges == null)
            return false;

        for (var i = 0; i < ranges.Count; i += 2)
        {
            if (DateTime.Compare(fromDate, ranges[i]) >= 0 && DateTime.Compare(toDate, ranges[i + 1]) <= 0)
            {
                return true;
            }
        }

        return false;
    }

    public void SaveToCache(List<TickerData> data, string symbol)
    {
        if (data.Count == 0) return;

        var cachedData = _tickerDictionary.GetValueOrDefault(symbol);
        var fromDate = data.Min(d => d.PriceDate);
        var toDate = data.Max(d => d.PriceDate);

        // Add in the retrieved ticker data to the cache
        if (cachedData is null)
        {
            cachedData = new List<TickerData>();
            cachedData.AddRange(data);
            cachedData.Sort();
        }
        else
        {
            foreach (var item in data)
            {
                if (!cachedData.Contains(item))
                {
                    cachedData.Add(item);
                }
            }

            cachedData.Sort();
        }

        _tickerDictionary[symbol] = cachedData;

        // Save the new range
        var ranges = _savedRanges.GetValueOrDefault(symbol);

        if (ranges is null || ranges.Count == 0)
        {
            ranges = new List<DateTime> { fromDate, toDate };

            _savedRanges[symbol] = ranges;
        }
        else
        {
            // add new range
            bool updated = false;
            for (var i = 0; i < ranges.Count; i += 2)
            {
                var a = ranges[i];
                var b = ranges[i + 1];

                if (DateTime.Compare(fromDate, a) >= 0 && DateTime.Compare(fromDate, b) <= 0 && DateTime.Compare(toDate, b) > 0)
                {
                    ranges[i + 1] = toDate;
                    updated = true;
                }
                else if (DateTime.Compare(toDate, a) >= 0 && DateTime.Compare(toDate, b) <= 0)
                {
                    ranges[i] = fromDate;
                    updated = true;
                }
            }

            if (!updated)
            {
                ranges.Add(fromDate);
                ranges.Add(toDate);
                ranges.Sort();
            }
            else
            {
                List<DateTime> toRemove = new List<DateTime>();
                for (var i = 1; i < ranges.Count - 1; i += 2)
                {
                    if (ranges[i] > ranges[i + 1])
                    {
                        toRemove.Add(ranges[i]);
                        toRemove.Add(ranges[i + 1]);
                    }
                }

                foreach (var item in toRemove)
                {
                    ranges.Remove(item);
                }
            }

            _savedRanges[symbol] = ranges;
        }
    }
}