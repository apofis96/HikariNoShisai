namespace HikariNoShisai.Common.Configs
{
    public class TelegramConfig
    {
        public required string Token { get; set; }
        public required string Url { get; set; }
        public required List<long> AllowedUsers { get; set; }
    }
}
