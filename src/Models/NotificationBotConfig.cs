namespace DozorBot.Models;

public class NotificationBotConfig
{
    public string Token { get; set; }
    public int MsgBatchLimit { get; set; } = 30;
    public int MsgLengthLimit { get; set; } = 4096;
    public bool UseProxy { get; set; } = false;
    public int SendingPeriod { get; set; } = 24; //24 hours by default if absent
    public int StoreMessagesPeriod { get; set; } = 24 * 30; //30 days by default if absent
    public ProxySettings ProxySettings { get; set; }
}

public class ProxySettings
{
    public string ProxyUrl { get; set; }
}