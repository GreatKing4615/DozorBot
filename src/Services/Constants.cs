namespace DozorBot.Services;

public class Constants
{
    public const string TELEGRAM_BOT_SETTINGS_KEY = "telegram_bot_settings";
    public const string TOKEN_IS_NULL = "Token can't be null";
    public const string CONTACT_STORED = "Your contact info was stored, just wait for notifications!";

    public const string error_message = @"Telegram Bot Settings not found found in DB
        Table ""settings""
        Record key ""telegram_bot_settings""
        Record value example:
        {
            ""token"": ""token_id:token_id_hash"",
            ""msg_batch_limit"": 30,
            ""msg_length_limit"": 4096,
            ""use_proxy"": false,
            ""proxy_settings"": {
            ""proxy_url"": ""http://username:password@proxy:port""
            }
        }";

    public const string empty_user_id = "telegram_user_id is empty";
}