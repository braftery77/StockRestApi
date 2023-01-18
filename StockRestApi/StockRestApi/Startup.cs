using Autofac;
using Autofac.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using StockRestApi.Modules;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Refit;
using StockRestApi.Apis;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Diagnostics;
using StockRestApi.Middleware;

namespace StockRestApi;

public class Startup
{
    IConfiguration Configuration { get; }
    IHostEnvironment HostEnvironment { get; }

    public Startup(IConfiguration configuration, IHostEnvironment env)
    {
        Configuration = configuration;
        HostEnvironment = env;

        JsonConvert.DefaultSettings = () =>
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = env.IsDevelopment() ? Formatting.Indented : Formatting.None
            };
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            return settings;
        };
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors()
            .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddScoped<IUrlHelper>(x => x
                .GetRequiredService<IUrlHelperFactory>()
                .GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext))
            .AddMvcCore()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = HostEnvironment.IsDevelopment();
            })
            .AddApiExplorer();

        services
            .AddAutoMapper(typeof(Startup)) 
            .AddSwagger();                 

        services.AddRouting();
        services.AddControllers();
        services.AddHealthChecks();

        services.AddRefitClient<IApperateApi>().ConfigureHttpClient((sp, c) =>
        {
            c.BaseAddress = new Uri("https://apis.iex.cloud/v1");
        });
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<DefaultModule>();
        builder.RegisterModule(new ConfigurationModule(Configuration));
    }

    public void ConfigureProductionContainer(ContainerBuilder builder)
    {
        ConfigureContainer(builder);
    }

    public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
    {
        app.UseSerilogRequestLogging();

        app.UseRouting();
        app.UseCors(builder => builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowAnyOrigin());

        app.UseOptionsVerbHandler()
           .UseSwaggerWithOptions();  

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
            endpoints.MapHealthChecks(Constants.Health.EndPoint);
        });

        logger.LogInformation("Server configuration is completed");
        var addr = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(addr))
        {
            var uri = new Uri(new Uri(addr), "swagger");
            logger.LogInformation("Open {uri} to browse the server API", uri);
        }
    }
}
