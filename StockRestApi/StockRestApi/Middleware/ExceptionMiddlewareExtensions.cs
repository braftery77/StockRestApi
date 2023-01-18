using Microsoft.AspNetCore.Builder;

namespace StockRestApi.Middleware;

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandler(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<Middleware.ExceptionMiddleware>();
    }
}
