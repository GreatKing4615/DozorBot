using DozorBot.DAL.Contracts;
using DozorBot.Infrastructure.Base;
using DozorBot.Models;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DozorBot.Services;

public class BotService : IBot
{
    private const string Start = "/start";
    private readonly ILog _log;
    private readonly ITelegramBotClient _botClient;
    private readonly IUnitOfWork<DozorDbContext> _unitOfWork;

    public BotService(ILog log, ITelegramBotClient botClient, IUnitOfWork<DozorDbContext> unitOfWork)
    {
        _log = log;
        _botClient = botClient;
        _unitOfWork = unitOfWork;
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions(),
            cancellationToken: default);
        _log.Info($"{nameof(BotService)} are ready");
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message)
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
        _log.Error($"{nameof(BotService)}: {JsonSerializer.Serialize(exception)}");
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
            var user = await _unitOfWork.GetRepository<AppUser>().SingleOrDefault(
                selector: x => new { x.Id, x.LegacyUser.UserName, x.LegacyUser.PhoneNumber, x.TelegramUserId },
                predicate: x => x.LegacyUser.PhoneNumber == cleanPhone,
                include: i => i.Include(x => x.LegacyUser));
            if (user == null)
            {
                var existedUser = await _unitOfWork.GetRepository<AppUser>().SingleOrDefault(
                     selector: x => x,
                     predicate: x => x.LegacyUser.PhoneNumber == cleanPhone,
                     include: i => i.Include(x => x.LegacyUser));
                if (existedUser != null)
                {
                    existedUser.TelegramUserId = update.Message?.Contact?.UserId;
                    _unitOfWork.GetRepository<AppUser>().Update(existedUser);
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
        var user = await _unitOfWork.GetRepository<AppUser>().SingleOrDefault(
            selector: x => x,
            predicate: x => x.TelegramUserId == chatId,
            include: i => i.Include(x => x.LegacyUser));
        if (user != null)
        {
            await _botClient.SendTextMessageAsync(chatId, "Your contact info was stored, just wait for notifications!");
            _log.Info($"Contact already exists for user {user.TelegramUserId}");
        }
        else
        {
            _log.Info($"Detected unknown telegram_id {chatId}");
            await NeedContact(update);
        }
    }
}