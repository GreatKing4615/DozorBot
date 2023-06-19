using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;

public static class LoggingConfig
{
    public static void ConfigureLogging(IServiceCollection services)
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
        services.AddSingleton<ILog>(LogManager.GetLogger(typeof(LoggingConfig)));
    }
}