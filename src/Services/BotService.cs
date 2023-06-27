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
    private readonly ILog _log;
    private ITelegramBotClient _botClient;
    private readonly IUnitOfWork<DozorDbContext> _unitOfWork;

    public BotService(ILog log, IUnitOfWork<DozorDbContext> unitOfWork)
    {
        _log = log;
        _unitOfWork = unitOfWork;
        _log.Info($"{nameof(BotService)} are ready");
    }

    public async Task StartListening(CancellationToken token)
    {
        _botClient = await TelegramBot.GetInstance(_unitOfWork, _log);
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions(),
            cancellationToken: default);
        _log.Info($"{nameof(BotService)} start listening");

        while (true)
        {
            _botClient = await TelegramBot.GetInstance(_unitOfWork, _log);
            await Task.Delay(60 * 1000, token);
        }
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message)
        {
            await Idle(update);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _log.Error($"{nameof(BotService)}: {JsonSerializer.Serialize(exception.Message)}");
        return Task.CompletedTask;
    }

    public async Task NeedContact(Update update)
    {
        string failMsg = "Bot needs your contact info to operate.";
        var message = update?.Message;

        //проверка контакта
        if (message.Contact == null)
        {
            await ReturnAnswer(update, failMsg);
            return;
        }

        var userPhone = message.Contact?.PhoneNumber;
        //проверка наличия номера
        if (userPhone == null)
        {
            failMsg = "There is no phone number!";
            await ReturnAnswer(update, failMsg);
            return;

        }

        var cleanPhone = Regex.Replace(userPhone, @"[^\d]", "");
        var userByPhone = await _unitOfWork.GetRepository<AppUser>().SingleOrDefault(
            selector: x => x,
            predicate: x => x.LegacyUser.PhoneNumber == cleanPhone,
            include: i => i.Include(x => x.LegacyUser));

        //проверка пользователя
        if (userByPhone == null)
        {
            failMsg = "You are not permitted to use this bot!";
            _log.Info($"Reject for unknown phone {message.Contact.PhoneNumber}");
            await ReturnAnswer(update, failMsg);
            return;
        }

        //проверка аккаунта пользователя
        if (userByPhone.TelegramUserId != default && userByPhone.TelegramUserId != message.Chat.Id)
        {
            failMsg = "It is not your contact!";
            _log.Info($"There is no phone number! for {message.Contact.UserId}");
            await ReturnAnswer(update, failMsg);
            return;
        }

        userByPhone.TelegramUserId = message.Contact?.UserId;
        _unitOfWork.GetRepository<AppUser>().Update(userByPhone);
        await _unitOfWork.SaveChangesAsync();
        _log.Info($"Store telegram_id {userByPhone.TelegramUserId} for user {userByPhone.Name} ");
        await Idle(update);
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
            await _botClient.SendTextMessageAsync(chatId, Constants.CONTACT_STORED, replyMarkup: new ReplyKeyboardRemove());
            _log.Info($"Contact already exists for user {user.TelegramUserId}");
        }
        else
        {
            _log.Info($"Detected unknown telegram_id {chatId}");
            await NeedContact(update);
        }
    }

    private async Task ReturnAnswer(Update update, string failMsg)
    {
        var buttons = new[]
        {
            new KeyboardButton("My phone number") {RequestContact = true},
            "Cancel"
        };
        var fulltext = $"{failMsg}\r";
        await _botClient.SendTextMessageAsync(
            chatId: update.Message!.Chat.Id,
            text: fulltext,
            replyMarkup: new ReplyKeyboardMarkup(buttons)
        );
    }

}