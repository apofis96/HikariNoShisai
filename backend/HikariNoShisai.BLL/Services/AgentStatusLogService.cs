using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.BLL.Services
{
    public class AgentStatusLogService(HikariNoShisaiContext context): IAgentStatusLogService
    {
        private readonly HikariNoShisaiContext _context = context;

        public async Task Create(AgentStatusLogRequest statusLog)
        {
            _context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = statusLog.AgentId,
                IsGridAvailable = statusLog.IsGridAvailable,
                GridVoltage = statusLog.GridVoltage,
                BatteryVoltage = statusLog.BatteryVoltage,
                CreatedAt = DateTimeOffset.UtcNow
            });
            
            await _context.SaveChangesAsync();
        }
    }
}
