using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HikariNoShisai.BLL.Services
{
    public class AgentService(HikariNoShisaiContext context, IMemoryCache memoryCache, IWeatherForecast weatherForecast) : IAgentService
    {
        private readonly HikariNoShisaiContext _context = context;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IWeatherForecast _weatherForecast = weatherForecast;
        private const string CacheKeyPrefix = "agent_";

        public async Task<IEnumerable<Agent>> GetAll()
        {
            var terminals = _context.Agents.Include(x => x.Terminals).AsNoTracking();

            return terminals;
        }

        public async Task<string?> GetNameById(Guid agentId)
        {
            var agentName = await _context.Agents
                .Where(x => x.Id == agentId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            return agentName;
        }

        public async Task<sbyte> GetWeather(Guid agentId)
        {
            if (!_memoryCache.TryGetValue<Agent>(CacheKeyPrefix + agentId, out var agent))
            {
                agent = await _context.Agents
                    .Where(x => x.Id == agentId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                _memoryCache.Set(CacheKeyPrefix + agentId, agent);
            }

            if (agent is null)
                return -99;

            var weather = await _weatherForecast.Get(agent!.Latitude, agent.Longitude);

            return (sbyte)weather.Main.Temp;
        }
    }
}
