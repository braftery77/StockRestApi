using Autofac;
using StockRestApi.Services;

namespace StockRestApi.Modules;

public class DefaultModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TickerService>().As<ITickerService>().SingleInstance();
        builder.RegisterType<ApperateApiService>().As<IApperateApiService>().SingleInstance();
        builder.RegisterType<TickerCache>().As<ITickerCache>().SingleInstance();
    }
}
