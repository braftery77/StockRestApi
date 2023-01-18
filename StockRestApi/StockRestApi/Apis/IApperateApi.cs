using Refit;
using StockRestApi.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockRestApi.Apis;

public interface IApperateApi
{

    [Get("/data/CORE/HISTORICAL_PRICES/{symbol}?from={fromDate}&to={toDate}&token=sk_5dd9d15b7f5841f2961cce485e0f6fa7")]
    public Task<List<TickerData>> Get(string symbol, DateTime fromDate, DateTime toDate);
}
