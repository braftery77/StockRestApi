using Microsoft.AspNetCore.Builder;

namespace StockRestApi.Middleware;

public static class OptionsVerbMiddlewareExtensions
{
    public static IApplicationBuilder UseOptionsVerbHandler(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<Middleware.OptionsVerbMiddleware>();
    }
}