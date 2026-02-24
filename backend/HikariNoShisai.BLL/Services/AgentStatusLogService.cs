using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.BLL.Services
{
    public class AgentStatusLogService(HikariNoShisaiContext context, IMessageQueue messageQueue) : IAgentStatusLogService
    {
        private readonly HikariNoShisaiContext _context = context;
        private readonly IMessageQueue _messageQueue = messageQueue;

        public async Task Create(AgentStatusLogRequest statusLog)
        {
            await EmitGridNotification(statusLog);

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

        private async Task EmitGridNotification(AgentStatusLogRequest statusLog)
        {
            var lastGridStatus = _context.AgentStatusLogs.OrderByDescending(x => x.CreatedAt).Select(x => x.IsGridAvailable).FirstOrDefault();
            if (lastGridStatus != statusLog.IsGridAvailable)
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(x => x.Id == statusLog.AgentId);
                if (agent is not null)
                {
                    var gridStatusNotification = new TelegramNotification
                    {
                        Message = string.Format(TextConstants.GridMessageTemplate, statusLog.IsGridAvailable ? "Available" : "Not Available"),
                        IsVerbose = false
                    };
                    _messageQueue.Send(MessageTopics.TelegramNotification, gridStatusNotification);
                }
            }
        }
    }
}
