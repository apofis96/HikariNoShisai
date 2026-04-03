using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HikariNoShisai.BLL.Services
{
    public class AgentTerminalService(HikariNoShisaiContext context, IMemoryCache memoryCache) : IAgentTerminalService
    {
        private readonly HikariNoShisaiContext _context = context;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private const string CacheKeyPrefix = "terminal_";

        public async Task<sbyte> GetAgentTerminalStatus(Guid agentId, Guid terminalId)
        {
            if (_memoryCache.TryGetValue<sbyte>(CacheKeyPrefix + agentId + terminalId, out var cachedStatus)) 
            {
                return cachedStatus;
            }
            var terminal = await GetAgentTerminal(_context, agentId, terminalId);
            if (terminal is null)
                return -1;

            var status = (sbyte)(terminal.IsActive ? 1 : 0);
            _memoryCache.Set(CacheKeyPrefix + agentId + terminalId, status);

            return status;
        }

        public async Task SetAgentTerminalStatus(Guid agentId, Guid terminalId, bool isActive)
        {
            var terminal = await GetAgentTerminal(_context, agentId, terminalId);
            if (terminal is null)
                return;

            terminal.IsActive = isActive;
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + agentId + terminalId);
        }

        public async Task ToggleAgentTerminalStatus(Guid agentId, Guid terminalId)
        {
            var terminal = await GetAgentTerminal(_context, agentId, terminalId);
            if (terminal is null)
                return;

            terminal.IsActive = !terminal.IsActive;
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + agentId + terminalId);
        }

        public async Task<Guid> GetAgentIdByTerminalId(Guid terminalId)
        {
            var terminal = await _context.AgentTerminals.Where(x => x.Id == terminalId).FirstOrDefaultAsync();
            return terminal?.AgentId ?? Guid.Empty;
        }

        private static readonly Func<HikariNoShisaiContext, Guid, Guid, Task<AgentTerminal?>> GetAgentTerminal =
        EF.CompileAsyncQuery((HikariNoShisaiContext context, Guid agentId, Guid terminalId) =>
            context.AgentTerminals
                .Where(x => x.AgentId == agentId && x.Id == terminalId)
                .FirstOrDefault()
        );
    }
}
