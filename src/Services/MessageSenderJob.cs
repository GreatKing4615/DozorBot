using DozorBot.DAL.Contracts;
using DozorBot.Infrastructure.Base;
using DozorBot.Models;
using DozorBot.Models.Enums;
using log4net;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;

namespace DozorBot.Services;

public class MessageSenderJob : IJob
{
    private ILog _log;
    private IUnitOfWork<DozorDbContext> _unitOfWork { get; set; }
    private NotificationBotConfig BotConfig { get; set; }
    private ITelegramBotClient _botClient { get; set; }
    private string _botName { get; set; }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        _unitOfWork = (IUnitOfWork<DozorDbContext>)dataMap.Get("UnitOfWork");
        _log = (ILog)dataMap.Get("Log");

        _botClient = await TelegramBot.GetInstance(_unitOfWork, _log);
        BotConfig = await TelegramBot.GetConfigFromDb(); 
        _botName = (await _botClient.GetMyNameAsync()).Name;
        await CbNotify();
    }
    
    public async Task CbNotify()
    {
        var messages = await _unitOfWork.GetRepository<TelegramMessage>().GetAll(
            selector: x => new { TelegramMessage = x, x.User },
            predicate: x => x.Status == MessageStatus.sending.ToString(),
            include: i => i.Include(x => x.User).ThenInclude(x=>x.LegacyUser)).ToListAsync();


        DeleteOldMessages(BotConfig.StoreMessagesPeriod);
        foreach (var msg in messages)
        {
            if (msg.User.TelegramUserId != null)
            {
                try
                {
                    await _botClient.SendTextMessageAsync(msg.User.TelegramUserId, msg.TelegramMessage.Text);
                    var messageToUpdate = await _unitOfWork.GetRepository<TelegramMessage>().SingleOrDefault(selector: x => x,
                        predicate: x => x.Id.ToString() == msg.TelegramMessage.Id.ToString());
                    messageToUpdate.Status = MessageStatus.ok.ToString();
                    _unitOfWork.GetRepository<TelegramMessage>().Update(messageToUpdate);
                    _log.Info(
                        $"Message id={msg.TelegramMessage.Id} sent for user={msg.User.Name} phone={msg.User.LegacyUser.PhoneNumber} text=\"{msg.TelegramMessage.Text}\"");
                }
                catch (Exception e)
                {
                    SetErrorInMessage(msg.TelegramMessage, MessageStatus.error, e.Message);
                }
            }
            else
                SetErrorInMessage(msg.TelegramMessage, MessageStatus.error, Constants.empty_user_id);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public void DeleteOldMessages(int ageHours)
    {
        if (ageHours <= 0) // disable deleting if ageHours < 0
            _log.Info($"{_botName}: Store period expired for 0 message(s) Deleted. AgeHours = {ageHours}");
        var oldMessages = _unitOfWork.GetRepository<TelegramMessage>().GetAll(
            selector: x => x,
            predicate: x => x.CreateDate < (DateTime.Now - TimeSpan.FromHours(ageHours))
        ).ToList();
        _unitOfWork.GetRepository<TelegramMessage>().DeleteRange(oldMessages);
        _log.Info($"{_botName}: Store period expired for {oldMessages.Count} message(s) Deleted. AgeHours = {ageHours}");
    }

    private void SetErrorInMessage(TelegramMessage msg, MessageStatus status, string error)
    {
        msg.Status = status.ToString();
        msg.Additional = error;
        _unitOfWork.GetRepository<TelegramMessage>().Update(msg);
        _log.Info(
            $"Message id={msg.Id} sent for user={msg.User.Name} phone={msg.User.LegacyUser.PhoneNumber} text=\"{msg.Text}\"");
    }
}