using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.BLL.Services
{
    public class AgentTerminalService(HikariNoShisaiContext context): IAgentTerminalService
    {
        private readonly HikariNoShisaiContext _context = context;

        public async Task<sbyte> GetAgentTerminalStatus(Guid agentId, Guid terminalId)
        {
            var isTerminalExists = await TerminalExistsAsync(_context, agentId, terminalId);
            if (!isTerminalExists)
                return -1;

            var isActive = await GetAgentTerminalStatusQuery(_context, agentId, terminalId);

            return (sbyte)(isActive ? 1 : 0);
        }

        private static readonly Func<HikariNoShisaiContext, Guid, Guid, Task<bool>> GetAgentTerminalStatusQuery =
        EF.CompileAsyncQuery((HikariNoShisaiContext context, Guid agentId, Guid terminalId) =>
            context.AgentTerminals
                .Where(x => x.AgentId == agentId && x.Id == terminalId)
                .Select(x => x.IsActive)
                .FirstOrDefault()
        );

        private static readonly Func<HikariNoShisaiContext, Guid, Guid, Task<bool>> TerminalExistsAsync =
        EF.CompileAsyncQuery((HikariNoShisaiContext context, Guid agentId, Guid terminalId) =>
            context.AgentTerminals
                .Any(e => e.AgentId == agentId && e.Id == terminalId)
        );
    }
}
