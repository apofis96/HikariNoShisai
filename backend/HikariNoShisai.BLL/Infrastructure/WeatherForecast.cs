using HikariNoShisai.Common.Configs;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace HikariNoShisai.BLL.Infrastructure
{
    public class WeatherForecast(
        IMemoryCache memoryCache,
        IOptions<OpenWeatherConfig> config,
        HttpClient httpClient,
        ILogger<WeatherForecast> logger) : IWeatherForecast
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly OpenWeatherConfig _config = config.Value;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<WeatherForecast> _logger = logger;
        private const string CacheKeyPrefix = "weather_";

        public async Task<Weather> Get(double latitude, double longitude)
        {
            var cacheKey = $"{CacheKeyPrefix}{latitude}_{longitude}";
            if (_memoryCache.TryGetValue(cacheKey, out Weather? weather) && weather is not null)
                return weather;

            var response = await _httpClient.GetAsync($"?lat={latitude}&lon={longitude}&units=metric&&appid={_config.ApiKey}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to fetch weather data: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                return GetFallbackWeather();
            }

            weather = await response.Content.ReadFromJsonAsync<Weather>();
            if (weather is null)
            {
                _logger.LogError("Deserialized weather payload was null. Returning fallback weather.");
                return GetFallbackWeather();
            }

            _memoryCache.Set(cacheKey, weather, TimeSpan.FromMinutes(_config.Cooldown));

            return weather;
        }

        private static Weather GetFallbackWeather() => new() { Main = new() { Temp = -99 } };
    }
}
