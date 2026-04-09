namespace HikariNoShisai.Common.Models
{
    public class TelegramHtmlMessage
    {
        public string HtmlContent { get; set; } = string.Empty;
        public IList<Stream>? Streams { get; set; } = null;
    }
}
