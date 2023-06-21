using System.Text.Json;
using DozorBot.DAL.Contracts;
using DozorBot.DAL.UnitOfWork;
using DozorBot.Infrastructure.Base;
using DozorBot.Models;
using DozorBot.Services;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;

[assembly: XmlConfigurator(Watch = true)]

namespace DozorBot;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        var smdConnectionString = configuration.GetConnectionString("DatabaseConnection");
        services.AddDbContext<DozorDbContext>(options =>
            options.UseNpgsql(smdConnectionString));

        services.AddUnitOfWork<DozorDbContext>();

        host.ConfigureServices((context, services) =>
        {
            services.AddOptions();
            services.AddSingleton(configuration);
        });


        await using var serviceProvider = services.BuildServiceProvider();
        XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));
        var log = LogManager.GetLogger(typeof(Program));
        services.AddSingleton(log);

        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork<DozorDbContext>>();
        var botConfig = configuration.GetSection("Config") as NotificationBotConfig;
        await SetConfigToDb(botConfig, log, unitOfWork);

        var smdBot = await TelegramBot.GetInstance(unitOfWork, log);

        var jobDataMap = new JobDataMap();
        jobDataMap.Put("UnitOfWork", unitOfWork);
        jobDataMap.Put("BotClient", smdBot);
        jobDataMap.Put("Log", log);
        jobDataMap.Put("BotConfig", botConfig);

        await StartSendingMessages(configuration, jobDataMap);
        var botService = new BotService(log, unitOfWork);
        await botService.StartListening(new CancellationToken());

        await host.Build().RunAsync();
        Console.WriteLine("Press any key to stop");
    }

    private static async Task SetConfigToDb(NotificationBotConfig config, ILog log, IUnitOfWork<DozorDbContext> unitOfWork)
    {
        if (config == null)
        {
            log.Info($"{nameof(TelegramBot)}: doesn't have config. Will be use default config");
            return;
        }

        var configStr = await unitOfWork.GetRepository<Settings>().SingleOrDefault(
            selector: x => x,
            predicate: x => x.Key == Constants.TELEGRAM_BOT_SETTINGS_KEY
        );
        if (!string.IsNullOrEmpty(configStr.Value))
            config.Token = JsonSerializer.Deserialize<NotificationBotConfig>(configStr.Value)?.Token;

        log.Info($"{nameof(TelegramBot)}: set config (without token) in db");
        configStr.Value = JsonSerializer.Serialize(config);
        unitOfWork.GetRepository<Settings>().Update(configStr);
    }

    private static async Task StartSendingMessages(
        IConfiguration configuration,
        JobDataMap dataMap)
    {
        var _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        var notifyCron = configuration["NotifyCron"] ?? "0 0/5 * 1/1 * ? *"; //default value - every 5 minute
        await _scheduler.Start();

        var job = JobBuilder.Create<MessageSenderJob>()
            .WithIdentity("messageSenderJob", "group")
            .UsingJobData(dataMap)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("messageSenderTrigger", "group")
            .WithCronSchedule(notifyCron)
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}