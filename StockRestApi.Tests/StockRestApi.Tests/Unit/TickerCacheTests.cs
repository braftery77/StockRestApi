using StockRestApi.Model;
using StockRestApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace StockRestApi.Tests.Unit;

public class TickerCacheTests
{
    private readonly TickerCache _sut;
    private Func<Task<List<TickerData>>> _callback;
    private bool _callbackHit;
    private readonly Random _rndm = new Random();

    public TickerCacheTests()
    {
        _callbackHit= false;
        _sut = new TickerCache();
    }

    [Fact]
    public async Task TickerCacheReturnsCalledDataIfNotCached()
    {
        var data = new List<TickerData>()
        {
            GenerateTicker(DateTime.Today)
        };

        _callback = () => MockApiFunc(data);

        var result = await _sut.GetOrAddAsync("TSLA", new DateTime(2022, 10, 12), new DateTime(2022, 10, 15), _callback);

        Assert.True(_callbackHit);
    }

    [Fact]
    public async Task TickerCacheReturnsSavedDataIfIsCached()
    {
        var dataToSave = new List<TickerData>()
        {
            GenerateTicker(DateTime.Today.AddDays(-1)),
            GenerateTicker(DateTime.Today.AddDays(-2)),
            GenerateTicker(DateTime.Today.AddDays(-3))
        };

        _sut.SaveToCache(dataToSave, "TSLA");

        var data = new List<TickerData>()
        {
            GenerateTicker(DateTime.Today)
        };

        _callback = () => MockApiFunc(data);

        var result = await _sut.GetOrAddAsync("TSLA", DateTime.Today.AddDays(-3), DateTime.Today.AddDays(-2), _callback);

        Assert.False(_callbackHit);
    }

    [Fact]
    public async Task TickerCacheConsolidatesDates()
    {
        var dataToSave = new List<TickerData>()
        {
            GenerateTicker(DateTime.Today.AddDays(-1)),
            GenerateTicker(DateTime.Today.AddDays(-2)),
            GenerateTicker(DateTime.Today.AddDays(-3))
        };

        var additionalDataToSave = new List<TickerData>()
        {
            GenerateTicker(DateTime.Today.AddDays(-3)),
            GenerateTicker(DateTime.Today.AddDays(-4)),
            GenerateTicker(DateTime.Today.AddDays(-5))
        };

        var ignoredCall = new List<TickerData>()
        {
            GenerateTicker(DateTime.Today.AddDays(-2)),
            GenerateTicker(DateTime.Today.AddDays(-3)),
            GenerateTicker(DateTime.Today.AddDays(-4))
        };

        _sut.SaveToCache(dataToSave, "TSLA");
        _sut.SaveToCache(additionalDataToSave, "TSLA");

        var firstCallback = () => MockApiFunc(dataToSave);
        var secondCallback = () => MockApiFunc(additionalDataToSave);
        var thirdCallback = () => MockApiFunc(ignoredCall);

        await _sut.GetOrAddAsync("TSLA", DateTime.Today.AddDays(-3), DateTime.Today.AddDays(-1), _callback);
        await _sut.GetOrAddAsync("TSLA", DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-3), _callback);

        _callbackHit = false; // reset flag to see if hit

        var result = await _sut.GetOrAddAsync("TSLA", DateTime.Today.AddDays(-4), DateTime.Today.AddDays(-2), _callback);

        Assert.False(_callbackHit);
        result.Capacity.Should().Be(3);
        result.Should().NotContainEquivalentOf(thirdCallback);
    }

    private Task<List<TickerData>> MockApiFunc(List<TickerData>? mockReturn)
    {
        var data = new List<TickerData>()
        {
            GenerateTicker(DateTime.Today)
        };

        _callbackHit = true;

        return Task.FromResult(mockReturn ?? data);
    }

    private TickerData GenerateTicker(DateTime? date)
    {
        return new TickerData
        {
            Symbol = "TSLA",
            PriceDate = date ?? DateTime.Today,
            Open = (decimal)_rndm.NextDouble(),
            Close = (decimal)_rndm.NextDouble()
        };
    }
}
