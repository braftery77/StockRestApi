using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockRestApi.Middleware;

public class ExceptionMiddleware
{
    RequestDelegate Next { get; }
    ILogger Logger { get; }
    IHostEnvironment Environment { get; }

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
    {
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var body = context.Response.Body;
        try
        {
            await Next(context);
        }
        catch (Exception ex)
        {
            context.Response.Body = body;
            await HandleExceptionAsync(context, ex);
        }
    }

    async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        int statusCode = 500;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        if (ex is KeyNotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }

        await context.Response.WriteAsync(
            JsonConvert.SerializeObject(
                new ErrorResponse(ex, Environment.IsDevelopment())));

        if (context.Response.StatusCode == StatusCodes.Status500InternalServerError)
        {
            Logger.LogError(ex, "Unhandled exception occurred");
        }
        else
        {
            Logger.LogDebug(ex, "Unhandled exception occurred");
        }
    }

    private class ErrorResponse
    {
        public ErrorResponse(Exception ex, bool includeFullExceptionInfo)
        {
            Error = new ExceptionDescription(ex);
            if (includeFullExceptionInfo)
            {
                Error.Exception = ex;
            }
        }

        public ExceptionDescription Error { get; set; }
    }

    private class ExceptionDescription
    {
        public ExceptionDescription(Exception ex)
        {
            Message = ex.Message;
        }

        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
