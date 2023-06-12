using DozorBot.DAL;
using DozorBot.Models;
using log4net;
using System.Net;
using System.Text.Json;
using DozorBot.DAL.Contracts;
using DozorBot.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DozorBot.Services;

public class BotService : IBot
{
    private readonly ILog _log;
    private readonly IRepository<TelegramMessage> _messageRepository;
    private readonly IRepository<Setting> _settingRepository;
    private readonly ITelegramBotClient _botClient;
    private readonly string _botName;
    private NotificationBotConfig BotConfig { get; set; }

    public BotService(ILog log, ITelegramBotClient botClient, IUnitOfWork unitOfWork)
    {
        _log = log;
        _botClient = botClient;
        _messageRepository = unitOfWork.GetRepository<TelegramMessage>();
        _settingRepository = unitOfWork.GetRepository<Setting>();
        _botName = _botClient.GetMeAsync().Result.FirstName;
        _log.Info($"{_botName} are ready");
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;
            if (message?.Text?.ToLower() == "/start")
            {
                await _botClient.SendTextMessageAsync(message.Chat, "Dozor bot are enabled!", cancellationToken: cancellationToken);
                return;
            }
            await _botClient.SendTextMessageAsync(message.Chat, "Reaction for update", cancellationToken: cancellationToken);
        }
    }

    public async Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }

    public int DeleteOldMessages(int ageHours)
    {
        if (ageHours <= 0) // disable deleting if ageHours < 0
            return 0;
        var oldMessages = _messageRepository.GetAll(
            selector: x => x,
            predicate: x => x.CreateDate < (DateTime.Now - TimeSpan.FromHours(ageHours))
            ).ToList();
        _messageRepository.DeleteRange(oldMessages);
        _log.Info($"{_botName}: Store period expired for {oldMessages.Count} message(s). Deleted");
        var test = _messageRepository.GetAll(
            selector: x => new {TelegramMessage = x, AppUserid = x.UserId, AspNetUserId = x.User.LegacyId},
            predicate: x => x.Status == MessageStatus.sending.ToString(),
            include: i => i.Include(x => x.User).ThenInclude(x => x.LegacyUser));
        return oldMessages.Count;
    }

    public async Task CbNotify()
    {
        await HealthCheck();
    }

    Task IBot.Error(Update update, Exception error)
    {
        throw new NotImplementedException();
    }

    Task IBot.Start(Update update)
    {
        throw new NotImplementedException();
    }

    Task IBot.NeedContact(Update update)
    {
        throw new NotImplementedException();
    }

    public void Error(Update update, Exception error)
    {
        throw new NotImplementedException();
    }

    public void Start(Update update)
    {
        throw new NotImplementedException();
    }

    public void NeedContact(Update update)
    {
        throw new NotImplementedException();
    }

    public async Task HealthCheck()
    {
        var configStr = await _settingRepository.SingleOrDefault(
            selector: x => x.Value,
            predicate: x => x.Key == Constants.TELEGRAM_BOT_SETTINGS_KEY
        );

        if (configStr == null)
        {
            _log.Error($"{_botName}: doesn't have setting in db");
            throw new ArgumentNullException(Constants.error_message);
        }
        var configFromDb = JsonSerializer.Deserialize<NotificationBotConfig>(configStr) ??
                     throw new JsonException($"{_botName}: can't convert json string");

        await ValidateToken(configFromDb);
        BotConfig = configFromDb;
    }

    private async Task ValidateToken(NotificationBotConfig config)
    {
        if (string.IsNullOrEmpty(config.Token))
        {
            var tokenError = $"{_botName}: doesn't have token in config";
            _log.Error(tokenError);
            throw new Exception(tokenError);
        }

        var apiResult = await _botClient.TestApiAsync();
        if (!apiResult)
        {
            throw new Exception($"{_botName}: status - unhealthy");
        }
    }
}