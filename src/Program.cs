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
        botConfig.Token = configuration["Config:Token"] ?? throw new ArgumentNullException(Constants.TOKEN_IS_NULL);
        if (int.TryParse(configuration["Config:MsgBatchLimit"], out var batchLimit))
            botConfig.MsgBatchLimit = batchLimit;
        if (int.TryParse(configuration["Config:MsgLengthLimit"], out var lengthLimit))
            botConfig.MsgLengthLimit = lengthLimit;
        if (int.TryParse(configuration["Config:StoreMessagesPeriod"], out var storePeriod))
            botConfig.StoreMessagesPeriod = storePeriod;
        if (int.TryParse(configuration["Config:SendingPeriod"], out var sendingPeriod))
            botConfig.SendingPeriod = sendingPeriod;

        var smdConnectionString = configuration.GetConnectionString("DatabaseConnection");
        services.AddDbContext<DozorDbContext>(options =>
            options.UseNpgsql(smdConnectionString));

        services.AddUnitOfWork<DozorDbContext>();
        var smdBot = new TelegramBotClient(botConfig.Token);


        host.ConfigureServices((context, services) =>
        {
            services.AddOptions();
        });


        await using var serviceProvider = services.BuildServiceProvider();

        var dozorDbContext = serviceProvider.GetRequiredService<DozorDbContext>();
        await dozorDbContext.Database.EnsureCreatedAsync();
        await dozorDbContext.Database.MigrateAsync();

        XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));
        var log = LogManager.GetLogger(typeof(Program));
        services.AddSingleton(log);

        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork<DozorDbContext>>();

        var jobDataMap = new JobDataMap();
        jobDataMap.Put("UnitOfWork", unitOfWork);
        jobDataMap.Put("BotClient", smdBot);
        jobDataMap.Put("Log", log);
        jobDataMap.Put("BotConfig", botConfig);

        new BotService(log, smdBot, unitOfWork);
        await StartSendingMessages(configuration, jobDataMap);

        await host.Build().RunAsync();
        Console.WriteLine("Press any key to stop");
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
