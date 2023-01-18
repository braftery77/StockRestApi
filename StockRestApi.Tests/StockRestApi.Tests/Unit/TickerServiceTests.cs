using Moq;
using StockRestApi.Model;
using StockRestApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StockRestApi.Tests.Unit;

public class TickerServiceTests
{
    private readonly Mock<ITickerCache> _tickerCache;
    private readonly Mock<IApperateApiService> _apperateApiService;
    private readonly TickerService _sut;
    public TickerServiceTests() 
    { 
        _tickerCache = new Mock<ITickerCache>();
        _apperateApiService = new Mock<IApperateApiService>();
        _sut = new TickerService(_apperateApiService.Object, _tickerCache.Object);
    }

    [Fact]
    public async Task GetTickerDataShouldCallCache()
    {
        _tickerCache.Setup(ts => ts.GetOrAddAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Func<Task<List<TickerData>>>>()))
            .ReturnsAsync(new List<TickerData>())
            .Verifiable();


        await _sut.GetTickerData("TSLA", DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1));

        _tickerCache.Verify();

    }
}
