using static HikariNoShisai.Common.Constants.TextConstants;

namespace HikariNoShisai.Common.Models
{
    public class TelegramNotification
    {
        public MessageTemplate Template { get; set; }
        public string[] Values { get; set; } = [];
        public bool IsVerbose { get; set; } = false;
    }
}
