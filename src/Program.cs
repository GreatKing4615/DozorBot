using DozorBot.DAL.UnitOfWork;
using DozorBot.Infrastructure.Base;
using DozorBot.Infrastructure.Logging;
using DozorBot.Services;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

var host = Host.CreateDefaultBuilder(args);
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
var services = new ServiceCollection();
// XmlConfigurator.Configure(new FileInfo("log4net.config"));

//depo
// var depoLoggerRepository = LogManager.GetRepository("DepoLogger");
// services.AddSingleton(depoLoggerRepository);
// services.AddScoped<ILog>(_ => new LogWrapper(LogManager.GetLogger("DepoLogger")));
var depoConnectionString = configuration.GetConnectionString("DepoConnection");
services.AddDbContext<DepoDbContext>(options =>
    options.UseNpgsql(depoConnectionString));
services.AddUnitOfWork<DepoDbContext>();
var depoBot = new TelegramBotClient(configuration["DepoTelegram:Token"]);

//smd
// var smdLoggerRepository = LogManager.GetRepository("SmdLogger");
// services.AddSingleton(smdLoggerRepository);
// services.AddScoped<ILog>(_ => new LogWrapper(LogManager.GetLogger("SmdLogger")));
var smdConnectionString = configuration.GetConnectionString("SmdArmConnection");
services.AddDbContext<SmdDbContext>(options =>
    options.UseNpgsql(smdConnectionString));
services.AddUnitOfWork<SmdDbContext>();
var smdBot = new TelegramBotClient(configuration["SmdTelegram:Token"]);



// //running bots
// var cancellationToken = new CancellationTokenSource().Token;
// depoBot.StartReceiving(
//     BotService.HandleUpdateAsync,
//     BotService.HandleErrorAsync,
//     new ReceiverOptions
//     {
//         AllowedUpdates = { },
//         Limit = Convert.ToInt32(configuration["DepoTelegram:Limit"]),
//         Offset = Convert.ToInt32(configuration["DepoTelegram:Offset"])
//     },
//     cancellationToken
// );
//
// smdBot.StartReceiving(
//     BotService.HandleUpdateAsync,
//     BotService.HandleErrorAsync,
//     new ReceiverOptions
//     {
//         AllowedUpdates = { },
//         Limit = Convert.ToInt32(configuration["SmdTelegram:Limit"]),
//         Offset = Convert.ToInt32(configuration["SmdTelegram:Offset"])
//     },
//     cancellationToken
// );

Console.WriteLine("Press any key to stop");
// Console.ReadLine();

host.Build().RunAsync();