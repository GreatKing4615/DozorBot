using DozorBot.DAL.Contracts;
using DozorBot.Models;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Update = Telegram.Bot.Types.Update;

namespace DozorBot.Services;

public class BotService : IBot
{
    private const string Start = "/start";
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
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            },
            cancellationToken: default);
        _log.Info($"{_botName} are ready");
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;
            if (message?.Text?.ToLower() == Start)
            {
                await botClient.SendTextMessageAsync(message.Chat, "Dozor bot are enabled!",
                    cancellationToken: cancellationToken);
                return;
            }
            await Idle(update);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _log.Error($"{_botName}: {JsonSerializer.Serialize(exception)}");
        return Task.CompletedTask;
    }

    public async Task NeedContact(Update update)
    {
        var userPhone = update.Message?.Contact?.PhoneNumber;
        const string text = "Bot needs your contact info to operate.";
        var failMsg = string.Empty;
        var buttons = new[]
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
            chatId: update.Message!.Chat.Id,
            text: fulltext,
            replyMarkup: new ReplyKeyboardMarkup(buttons)
        );
    }

    public async Task Idle(Update update)
    {
        var chatId = update.Message?.Chat.Id;
        var user = await _userRepository.SingleOrDefault(
            selector: x => x.LegacyUser.UserName,
            predicate: x => x.TelegramUserId == chatId,
            include: i => i.Include(x => x.LegacyUser));
        if (user != null)
        {
            await _botClient.SendTextMessageAsync(chatId, "Your contact info was stored, just wait for notifications!");
            _log.Info($"Contact already exists for user {user}");
        }
        else
        {
            _log.Info($"Detected unknown telegram_id {chatId}");
            await NeedContact(update);
        }
    }
}