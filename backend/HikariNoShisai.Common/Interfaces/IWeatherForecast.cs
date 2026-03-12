using HikariNoShisai.Common.Models;

namespace HikariNoShisai.Common.Interfaces
{
    public interface IWeatherForecast
    {
        Task<Weather> Get(double latitude, double longitude);
    }
}
