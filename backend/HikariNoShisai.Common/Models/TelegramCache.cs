using HikariNoShisai.Common.Constants;

namespace HikariNoShisai.Common.Models
{
    public class TelegramCache
    {
        public TelegramChatStep ChatStep { get; set; }
        public object? Data { get; set; } = null;
    }
}
