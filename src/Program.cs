using DozorBot.DAL.Contracts;
using DozorBot.DAL.UnitOfWork;
using DozorBot.Infrastructure.Base;
using DozorBot.Models;
using DozorBot.Services;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

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
        var botConfig = new NotificationBotConfig();
        botConfig.Token = configuration["SmdTelegram:Token"] ?? botConfig.Token;
        if (int.TryParse(configuration["Config:MsgBatchLimit"], out var batchLimit))
            botConfig.MsgBatchLimit = batchLimit;
        if (int.TryParse(configuration["Config:MsgLengthLimit"], out var lengthLimit))
            botConfig.MsgLengthLimit = lengthLimit;
        if (int.TryParse(configuration["Config:StoreMessagesPeriod"], out var storePeriod))
            botConfig.StoreMessagesPeriod = storePeriod;
        if (int.TryParse(configuration["Config:SendingPeriod"], out var sendingPeriod))
            botConfig.SendingPeriod = sendingPeriod;
        
        var smdConnectionString = configuration.GetConnectionString("SmdArmConnection");
        services.AddDbContext<SmdDbContext>(options =>
            options.UseNpgsql(smdConnectionString));

        services.AddUnitOfWork<SmdDbContext>();
        var smdBot = new TelegramBotClient(configuration["SmdTelegram:Token"]);


        host.ConfigureServices((context, services) =>
        {
            services.AddOptions();
            LoggingConfig.ConfigureLogging(services);
        });
        

        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            var smdDbContext = scope.ServiceProvider.GetRequiredService<SmdDbContext>();
            await smdDbContext.Database.EnsureCreatedAsync();
            await smdDbContext.Database.MigrateAsync();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<SmdDbContext>>();
            var log = scope.ServiceProvider.GetRequiredService<ILog>();
            new BotService(log, smdBot, unitOfWork);
        }
        
        await StartSendingMessages(configuration, serviceProvider, smdBot);
        await host.Build().RunAsync();
        Console.WriteLine("Press any key to stop");
    }

    private static async Task StartSendingMessages(IConfigurationRoot configuration, ServiceProvider serviceProvider, ITelegramBotClient smdBot)
    {
        var _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        var notifyCron = configuration["NotifyCron"] ?? "0 0/5 * 1/1 * ? *"; //default value - every 5 minute
        await _scheduler.Start();

        var jobDataMap = new JobDataMap();
        jobDataMap.Put("UnitOfWork", serviceProvider.GetRequiredService<IUnitOfWork<SmdDbContext>>()); // Передача UnitOfWork в JobDataMap
        jobDataMap.Put("BotClient", smdBot); // Передача UnitOfWork в JobDataMap
        jobDataMap.Put("Log", serviceProvider.GetRequiredService<ILog>()); // Передача UnitOfWork в JobDataMap


        var job = JobBuilder.Create<MessageSenderJob>()
            .WithIdentity("messageSenderJob", "group")
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("messageSenderTrigger", "group")
            .WithCronSchedule(notifyCron)
        .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}
