using DozorBot.DAL.Contracts;
using DozorBot.Models;
using DozorBot.Models.Enums;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DozorBot.Services;

public class BotService : IBot
{
    private readonly ILog _log;
    private readonly IRepository<TelegramMessage> _messageRepository;
    private readonly IRepository<Setting> _settingRepository;
    private readonly IRepository<AppUser> _userRepository;
    private readonly ITelegramBotClient _botClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _botName;
    private NotificationBotConfig BotConfig { get; set; }

    public BotService(ILog log, ITelegramBotClient botClient, IUnitOfWork unitOfWork)
    {
        _log = log;
        _botClient = botClient;
        _unitOfWork = unitOfWork;
        _messageRepository = unitOfWork.GetRepository<TelegramMessage>();
        _settingRepository = unitOfWork.GetRepository<Setting>();
        _userRepository = unitOfWork.GetRepository<AppUser>();
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
                await _botClient.SendTextMessageAsync(message.Chat, "Dozor bot are enabled!",
                    cancellationToken: cancellationToken);
                return;
            }

            await _botClient.SendTextMessageAsync(message.Chat, "Reaction for update",
                cancellationToken: cancellationToken);
        }
    }

    public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        _log.Error($"{_botName}: {JsonSerializer.Serialize(exception)}");
        return Task.CompletedTask;
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
        return oldMessages.Count;
    }

    public async Task CbNotify()
    {
        await HealthCheck();
        var test = _messageRepository.GetAll(
            selector: x => new { TelegramMessage = x, x.User },
            predicate: x => x.Status == MessageStatus.sending.ToString(),
            include: i => i.Include(x => x.User).ThenInclude(x => x.LegacyUser));

        foreach (var msg in test)
        {
            if (msg.User.TelegramUserId != null)
            {
                try
                {
                    await _botClient.SendTextMessageAsync(msg.User.TelegramUserId, msg.TelegramMessage.Text);
                    msg.TelegramMessage.Status = MessageStatus.ok.ToString();
                    _messageRepository.Update(msg.TelegramMessage);
                    _log.Info(
                        $"Message id={msg.TelegramMessage.Id} sent for user={msg.User.Name} phone={msg.User.LegacyUser.PhoneNumber} text=\"{msg.TelegramMessage.Text}\"");
                }
                catch (Exception e)
                {
                    SetErrorInMessage(msg.TelegramMessage, MessageStatus.error, e.Message);
                }
            }
            else
            {
                SetErrorInMessage(msg.TelegramMessage, MessageStatus.error, Constants.empty_user_id);
            }

        }
    }

    Task IBot.Error(Update update, Exception error)
    {
        _log.Warn($"Update \"{update}\" caused error \"{error}\"");
        return Task.CompletedTask;
    }

    public Task Start(Update update)
    {
        throw new NotImplementedException();
    }

    public async Task NeedContact(Update update)
    {
        var userPhone = update.Message?.Contact?.PhoneNumber;
        var text = "Bot needs your contact info to operate.";
        var failMsg = string.Empty;
        var buttons = new KeyboardButton[]
        {
            new KeyboardButton("My phone number") { RequestContact = true },
            "Cancel"
        };


        if (!string.IsNullOrEmpty(userPhone)
            && update.Message?.Chat.Id != default)
        {
            var cleanPhone = Regex.Replace(userPhone, @"[^\d]", "");
            var user = await _userRepository.SingleOrDefault(
                selector: x => new { x.Id, x.LegacyUser.UserName, x.LegacyUser.PhoneNumber, x.TelegramUserId },
                predicate: x => x.LegacyUser.PhoneNumber == cleanPhone,
                include: i => i.Include(x => x.LegacyUser));
            if (user == null)
            {
                var existedUser = await _userRepository.SingleOrDefault(
                     selector: x => x,
                     predicate: x => x.LegacyUser.PhoneNumber == cleanPhone,
                     include: i => i.Include(x => x.LegacyUser));
                if (existedUser != null)
                {
                    existedUser.TelegramUserId = update.Message?.Contact?.UserId;
                    _userRepository.Update(existedUser);
                    await _unitOfWork.SaveChangesAsync();
                    _log.Info($"Store telegram_id {existedUser.TelegramUserId} for user {existedUser.Name} ");
                }
                else
                {
                    failMsg = "You are not permitted to use this bot!";
                    _log.Warn($"Reject for unknown phone {userPhone}");
                }
            }
            else
            {
                failMsg = "It is not your contact!";
                _log.Warn($"It is not your contact! for {userPhone}");
            }
        }
        else
        {
            failMsg = "There is no phone number!";
            _log.Warn($"There is no phone number! for {userPhone}");
        }

        var fulltext = $"{failMsg}\r\n{text}";
        await _botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: fulltext,
            replyMarkup: new ReplyKeyboardMarkup(buttons)
        );
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

        await ValidateToken(configFromDb.Token);
        BotConfig = configFromDb;
    }

    private void SetErrorInMessage(TelegramMessage msg, MessageStatus status, string error)
    {
        msg.Status = status.ToString();
        msg.Additional = error;
        _messageRepository.Update(msg);
        _log.Info(
            $"Message id={msg.Id} sent for user={msg.User.Name} phone={msg.User.LegacyUser.PhoneNumber} text=\"{msg.Text}\"");
    }


    private async Task ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            var tokenError = $"{_botName}: doesn't have token in token";
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