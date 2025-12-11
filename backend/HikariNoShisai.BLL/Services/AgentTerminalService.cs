using HikariNoShisai.Common.Entities;
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
            var terminal = await GetAgentTerminal(_context, agentId, terminalId);
            if (terminal is null)
                return -1;

            return (sbyte)(terminal.IsActive ? 1 : 0);
        }

        public async Task SetAgentTerminalStatus(Guid agentId, Guid terminalId, bool isActive)
        {
            var terminal = await GetAgentTerminal(_context, agentId, terminalId);
            if (terminal is null)
                return;

            terminal.IsActive = isActive;
            await _context.SaveChangesAsync();
        }

        public async Task ToggleAgentTerminalStatus(Guid agentId, Guid terminalId)
        {
            var terminal = await GetAgentTerminal(_context, agentId, terminalId);
            if (terminal is null)
                return;

            terminal.IsActive = !terminal.IsActive;
            await _context.SaveChangesAsync();
        }

        private static readonly Func<HikariNoShisaiContext, Guid, Guid, Task<AgentTerminal?>> GetAgentTerminal =
        EF.CompileAsyncQuery((HikariNoShisaiContext context, Guid agentId, Guid terminalId) =>
            context.AgentTerminals
                .Where(x => x.AgentId == agentId && x.Id == terminalId)
                .FirstOrDefault()
        );
    }
}
