using DozorBot.DAL.Contracts;
using DozorBot.Infrastructure.Base;
using DozorBot.Models;
using log4net;
using System.Text.Json;
using Telegram.Bot;

namespace DozorBot.Services;

public class TelegramBot
{
    private static ITelegramBotClient _instance;
    private static IUnitOfWork<DozorDbContext> _unitOfWork { get; set; }
    private static ILog _log { get; set; }

    private TelegramBot() {
    }

    public static async Task<ITelegramBotClient> GetInstance(IUnitOfWork<DozorDbContext> unitOfWork, ILog log)
    {
        _unitOfWork = unitOfWork;
        _log = log;

        var config = await GetConfigFromDb();
        if (_instance == null)
            _instance = new TelegramBotClient(config.Token);

        while (await ValidateToken() == false)
        {
            var delaySec = 60 * 1000; // by default 1 min
            _log.Warn($"{nameof(TelegramBot)}: delay {delaySec} sec");
            await Task.Delay(delaySec);
        }
        return _instance;
    }

    public static async Task<NotificationBotConfig?> GetConfigFromDb()
    {
        var configStr = await _unitOfWork.GetRepository<Settings>().SingleOrDefault(
            selector: x => x.Value,
            predicate: x => x.Key == Constants.TELEGRAM_BOT_SETTINGS_KEY
        );

        if (configStr == null)
        {
            _log.Warn($"{nameof(TelegramBot)}: doesn't have setting in db. Config will save from appsettings");
            var defaultConfig = new NotificationBotConfig();
            await _unitOfWork.GetRepository<Settings>().InsertAsync(new Settings
            {
                Key = Constants.TELEGRAM_BOT_SETTINGS_KEY,
                Value = JsonSerializer.Serialize(defaultConfig)
            });
            await _unitOfWork.SaveChangesAsync();
            return defaultConfig;
        }

        var configFromDb = JsonSerializer.Deserialize<NotificationBotConfig>(configStr) ??
                           throw new JsonException($"{nameof(TelegramBot)}: can't convert json string");

        while (string.IsNullOrEmpty(configFromDb.Token))
        {
            var delaySec = 60 * 1000; // by default 1 min
            _log.Warn($"{nameof(TelegramBot)}: didn't find token in config. delay {delaySec} sec");
            await Task.Delay(delaySec);
        }
        return configFromDb;
    }

    private static async Task<bool> ValidateToken()
    {
        try
        {
            await _instance.TestApiAsync();
        }
        catch (Exception e)
        {
            var error = $"{nameof(TelegramBot)}: test api return error";
            _log.Error(error, e);
            return false;
        }
        return true;
    }
}