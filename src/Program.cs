using DozorBot.DAL.UnitOfWork;
using DozorBot.Infrastructure.Base;
using DozorBot.Services;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

class Program
{
    static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();

        var smdConnectionString = configuration.GetConnectionString("SmdArmConnection");
        services.AddDbContext<SmdDbContext>(options =>
            options.UseNpgsql(smdConnectionString));
        var depoConnectionString = configuration.GetConnectionString("DepoConnection");
        services.AddDbContext<DepoDbContext>(options =>
            options.UseNpgsql(depoConnectionString));

        services.AddUnitOfWork<SmdDbContext>();
        services.AddUnitOfWork<DepoDbContext>();

        var depoBot = new TelegramBotClient(configuration["DepoTelegram:Token"]);
        var smdBot = new TelegramBotClient(configuration["SmdTelegram:Token"]);


        host.ConfigureServices((context, services) =>
        {
            services.AddOptions();
            services.AddLogging();
            services.AddScoped(_ => LogManager.GetLogger(typeof(BotService)));
            services.AddSingleton(configuration);
            services.AddSingleton(depoBot);
            services.AddSingleton(smdBot);
            services.AddScoped<IBot, BotService>();
            services.AddSingleton<ITelegramBotClient>(depoBot);
            services.AddSingleton<ITelegramBotClient>(smdBot);
        });

        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            var smdDbContext = scope.ServiceProvider.GetRequiredService<SmdDbContext>();
            smdDbContext.Database.EnsureCreated();
            smdDbContext.Database.Migrate();

            var depoDbContext = scope.ServiceProvider.GetRequiredService<DepoDbContext>();
            depoDbContext.Database.EnsureCreated();
            depoDbContext.Database.Migrate();
        }

        host.Build().Run();
        Console.WriteLine("Press any key to stop");
    }
}
