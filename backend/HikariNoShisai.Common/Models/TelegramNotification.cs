namespace HikariNoShisai.Common.Models
{
    public class TelegramNotification
    {
        public string Message { get; set; } = string.Empty;
        public bool IsVerbose { get; set; } = false;
    }
}
