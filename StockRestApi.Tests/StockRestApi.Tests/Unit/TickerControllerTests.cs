using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StockRestApi.Controllers;
using StockRestApi.Model;
using StockRestApi.Services;
using Xunit;

namespace StockRestApi.Tests.Unit;

public class TickerControllerTests
{
    private readonly TickerController _sut;
    private readonly Mock<ITickerService> _tickerService;

    public TickerControllerTests()
    {
        _tickerService = new Mock<ITickerService>();
        _sut = new TickerController(_tickerService.Object);
    }

    [Fact]
    public async Task SuccessReturns200Ok()
    {
        _tickerService.Setup(ts => ts.GetTickerData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(new List<TickerData>() { new TickerData() });

        var actionResult = await _sut.Get("TSLA", new DateTime(2022, 10, 25), new DateTime(2022, 10, 27));

        actionResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EmptySymbolReturns400BadRequest()
    {
        _tickerService.Setup(ts => ts.GetTickerData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(new List<TickerData>());

        var actionResult = await _sut.Get("", new DateTime(2022, 10, 23), new DateTime(2022, 10, 25));

        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = actionResult as BadRequestObjectResult;
        badRequest!.Value.Should().BeEquivalentTo("Ticker cannot be empty");
    }

    [Fact]
    public async Task InvalidDateOrderReturns400BadRequest()
    {
        _tickerService.Setup(ts => ts.GetTickerData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(new List<TickerData>());

        var actionResult = await _sut.Get("TSLA", new DateTime(2022, 9, 23), new DateTime(2022, 8, 25));

        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = actionResult as BadRequestObjectResult;
        badRequest!.Value.Should().BeEquivalentTo("From date must be earlier than the to date");
    }

    [Fact]
    public async Task InvalidDateRangeSizeReturns400BadRequest()
    {
        _tickerService.Setup(ts => ts.GetTickerData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(new List<TickerData>());

        var actionResult = await _sut.Get("TSLA", new DateTime(2017, 9, 23), new DateTime(2022, 8, 25));

        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = actionResult as BadRequestObjectResult;
        (badRequest!.Value as string).Should().Contain("Max date range to pull is");
    }

    [Fact]
    public async Task EmptyResultReturns204NoContent()
    {
        _tickerService.Setup(ts => ts.GetTickerData(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(new List<TickerData>());

        var actionResult = await _sut.Get("TSLA", new DateTime(2022, 8, 23), new DateTime(2022, 8, 25));

        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Theory]
    [InlineData(17, 19, 20, 20)]
    [InlineData(18, 19, 20, 20)]
    [InlineData(15, 15, 17, 16)]
    [InlineData(15, 15, 18, 16)]
    public async Task DatesOnWeekendMovedToCorrectWeekday(int fromDay, int expectedFrom, int toDay, int expectedTo)
    {
        var args = new List<DateTime>();
        _tickerService.Setup(ts => ts.GetTickerData(It.IsAny<string>(), Capture.In(args), Capture.In(args)))
                      .ReturnsAsync(new List<TickerData>());

        var actionResult = await _sut.Get("TSLA", new DateTime(2022, 12, fromDay), new DateTime(2022, 12, toDay));

        args[0].Should().HaveDay(expectedFrom);
        args[1].Should().HaveDay(expectedTo);
    }
}
