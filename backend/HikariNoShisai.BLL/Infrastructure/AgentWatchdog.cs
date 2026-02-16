using HikariNoShisai.Common.Interfaces;
using System.Collections.Concurrent;

namespace HikariNoShisai.BLL.Infrastructure
{
    public class AgentWatchdog : IAgentWatchdog
    {
        private static readonly ConcurrentDictionary<Guid, DateTime> agents = new();

        public void Update(Guid id)
        {
            agents.AddOrUpdate(id, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
        }

        public IEnumerable<Guid> GetExpired(TimeSpan interval)
        {
            var threshold = DateTime.UtcNow - interval;

            var expiredAgents = agents.Where(kv => kv.Value < threshold).Select(kv => kv.Key);
            expiredAgents.ToList().ForEach(id => agents.TryRemove(id, out _));

            return expiredAgents;
        }
    }
}
