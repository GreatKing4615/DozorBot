using Telegram.Bot;
using Telegram.Bot.Types;

namespace DozorBot.Services;

public interface IBot
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
    Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken);
    int DeleteOldMessages(int ageHours);

    Task CbNotify();
    Task Error(Update update, Exception error);
    Task Start(Update update);
    Task NeedContact(Update update);
}