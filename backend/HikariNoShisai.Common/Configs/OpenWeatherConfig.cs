namespace HikariNoShisai.Common.Configs
{
    public class OpenWeatherConfig
    {
        public required string BaseUrl { get; set; }
        public required string ApiKey { get; set; }
        public required int Cooldown { get; set; }
    }
}
