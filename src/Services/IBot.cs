using Telegram.Bot;
using Telegram.Bot.Types;

namespace DozorBot.Services;

public interface IBot
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
    
    Task NeedContact(Update update);
    Task Idle(Update update);
}