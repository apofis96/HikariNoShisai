namespace HikariNoShisai.Common.Models
{
    public class Weather
    {
        public required MainWeather Main { get; set; }

        public class MainWeather
        {
            public double Temp { get; set; }
        }
    }
}
