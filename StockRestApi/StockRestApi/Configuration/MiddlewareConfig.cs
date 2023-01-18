using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace StockRestApi;

public static class MiddlewareConfig
{
    public static IApplicationBuilder UseSwaggerWithOptions(this IApplicationBuilder app)
    {
        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
            {
                if (!httpRequest.Headers.ContainsKey("X-Forwarded-Host")) return;

                var serverUrl = $"{httpRequest.Headers["X-Forwarded-Proto"]}://" +
                    $"{httpRequest.Headers["X-Forwarded-Host"]}" +
                    $"{httpRequest.Headers["X-Forwarded-Prefix"]}";

                swaggerDoc.Servers = new List<OpenApiServer>()
                {
                    new OpenApiServer { Url = serverUrl }
                };
            });
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(Constants.Swagger.EndPoint, Constants.Swagger.ApiName);
        });

        return app;
    }
}
