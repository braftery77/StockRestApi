using Microsoft.AspNetCore.Mvc;
using StockRestApi.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StockRestApi.Controllers;

[Route("/ticker")]
[ApiController]
[Produces("application/json")]
public class TickerController : ControllerBase
{
    private const int MAX_HISTORY_YEARS = 4;
    private const double DAYS_IN_YEAR = 365.25;

    private readonly ITickerService _tickerService;

    public TickerController(ITickerService tickerService)
    {
        _tickerService = tickerService;
    }

    [HttpGet]
    [Route("{symbol}")]
    public async Task<IActionResult> Get([FromRoute] string symbol, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var ts = Stopwatch.StartNew();

        if (string.IsNullOrEmpty(symbol))
        {
            return BadRequest("Ticker cannot be empty");
        }

        if (fromDate is null)
        {
            if (toDate is null)
            {
                toDate = DateTime.Today;
            }

            fromDate = toDate.Value.AddYears(-1); // Default to 1 year ago
        }
        else if (toDate is null) 
        {
            toDate = (DateTime.Today - fromDate.Value).TotalDays / DAYS_IN_YEAR > MAX_HISTORY_YEARS ? 
                fromDate.Value.AddYears(MAX_HISTORY_YEARS) 
                : DateTime.Today;
        }


        if ((toDate.Value - fromDate.Value).TotalDays > MAX_HISTORY_YEARS * DAYS_IN_YEAR)
        {
            return BadRequest($"Max date range to pull is {MAX_HISTORY_YEARS} years");
        }

        if (DateTime.Compare(fromDate.Value, toDate.Value) > 0)
        {
            return BadRequest("From date must be earlier than the to date");
        }

        // Always convert from/to to nearest business day
        toDate = toDate.Value.DayOfWeek == DayOfWeek.Saturday ? toDate.Value.AddDays(-1) : toDate.Value.DayOfWeek == DayOfWeek.Sunday ? toDate.Value.AddDays(-2) : toDate;
        fromDate = fromDate.Value.DayOfWeek == DayOfWeek.Saturday ? fromDate.Value.AddDays(2) : fromDate.Value.DayOfWeek == DayOfWeek.Sunday ? fromDate.Value.AddDays(1) : fromDate;

        var result = await _tickerService.GetTickerData(symbol, fromDate.Value, toDate.Value);

        return result.Count > 0 ? Ok(result) : NoContent();
    }
}