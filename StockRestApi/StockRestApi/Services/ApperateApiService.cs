using Microsoft.Extensions.Options;
using Refit;
using StockRestApi.Apis;
using StockRestApi.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockRestApi.Services;

public interface IApperateApiService
{
    public Task<List<TickerData>> Get(string symbol, DateTime fromDate, DateTime toDate);
}

public class ApperateApiService : IApperateApiService
{
    private readonly IApperateApi _apperateApi;

    public ApperateApiService()
    {
        _apperateApi = RestService.For<IApperateApi>("https://apis.iex.cloud/v1");
    }

    public Task<List<TickerData>> Get(string symbol, DateTime fromDate, DateTime toDate)
    {
        return _apperateApi.Get(symbol, fromDate, toDate);
    }
}
