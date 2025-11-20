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
            var terminals = await GetAgents(_context).ToListAsync();

            return terminals;
        }

        private static readonly Func<HikariNoShisaiContext, IAsyncEnumerable<Agent>> GetAgents =
        EF.CompileAsyncQuery((HikariNoShisaiContext context) =>
            context.Agents.Include(x => x.Terminals).AsNoTracking()
        );
    }
}
