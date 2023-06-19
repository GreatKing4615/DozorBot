using DozorBot.DAL.Contracts;
using DozorBot.Models;
using DozorBot.Models.Enums;
using DozorBot.Services;
using log4net;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Text.Json;
using Telegram.Bot;

public class MessageSenderJob : IJob
{
    private ILog _log;
    private IRepository<TelegramMessage> _messageRepository;
    private IRepository<Setting> _settingRepository;
    private ITelegramBotClient _botClient;
    private IUnitOfWork _unitOfWork { get; set; }
    private readonly string _botName;
    private NotificationBotConfig BotConfig { get; set; }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        _unitOfWork = (IUnitOfWork)dataMap.Get("UnitOfWork");
        _messageRepository = _unitOfWork.GetRepository<TelegramMessage>();
        _settingRepository = _unitOfWork.GetRepository<Setting>();
        _botClient = (ITelegramBotClient)dataMap.Get("BotClient");
        _log = (ILog)dataMap.Get("Log");
        await CbNotify();
    }

    public async Task CbNotify()
    {
        await HealthCheck();
        var messages = _messageRepository.GetAll(
            selector: x => new { TelegramMessage = x, x.User },
            predicate: x => x.Status == MessageStatus.sending.ToString(),
            include: i => i.Include(x => x.User).ThenInclude(x => x.LegacyUser));

        DeleteOldMessages(BotConfig.StoreMessagesPeriod);
        foreach (var msg in messages)
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
    public void DeleteOldMessages(int ageHours)
    {
        if (ageHours <= 0) // disable deleting if ageHours < 0
            _log.Info($"{_botName}: Store period expired for 0 message(s) Deleted. AgeHours = {ageHours}");
        var oldMessages = _messageRepository.GetAll(
            selector: x => x,
            predicate: x => x.CreateDate < (DateTime.Now - TimeSpan.FromHours(ageHours))
        ).ToList();
        _messageRepository.DeleteRange(oldMessages);
        _log.Info($"{_botName}: Store period expired for {oldMessages.Count} message(s) Deleted. AgeHours = {ageHours}");
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
        BotConfig.Token = configFromDb.Token;
        await _unitOfWork.SaveChangesAsync();
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