using StockRestApi.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockRestApi.Services;

public interface ITickerService
{
    public Task<List<TickerData>> GetTickerData(string symbol, DateTime fromDate, DateTime toDate);
}
public class TickerService : ITickerService
{
    private readonly IApperateApiService _apperateApiService;
    private readonly ITickerCache _tickerCache;

    public TickerService(IApperateApiService apperateApiService, ITickerCache tickerCache)
    {
        _apperateApiService = apperateApiService;
        _tickerCache = tickerCache;
    }

    public async Task<List<TickerData>> GetTickerData(string symbol, DateTime fromDate, DateTime toDate)
    {
        return await _tickerCache.GetOrAddAsync(symbol, fromDate, toDate, async () => (await _apperateApiService.Get(symbol, fromDate, toDate)));
    }
}