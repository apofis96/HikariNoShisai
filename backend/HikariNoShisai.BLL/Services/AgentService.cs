using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.BLL.Services
{
    public class AgentService(HikariNoShisaiContext context): IAgentService
    {
        private readonly HikariNoShisaiContext _context = context;

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
    }
}
