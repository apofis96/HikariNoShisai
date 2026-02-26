using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Helpers;
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
            var dateNow = DateTimeOffset.UtcNow;
            await EmitGridNotification(statusLog, dateNow);

            _context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = statusLog.AgentId,
                IsGridAvailable = statusLog.IsGridAvailable,
                GridVoltage = statusLog.GridVoltage,
                BatteryVoltage = statusLog.BatteryVoltage,
                CreatedAt = dateNow,
            });
            
            await _context.SaveChangesAsync();
        }

        private async Task EmitGridNotification(AgentStatusLogRequest statusLog, DateTimeOffset dateNow)
        {
            var lastAgentStatus = _context.AgentStatusLogs.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            if (lastAgentStatus is not null && lastAgentStatus.IsGridAvailable != statusLog.IsGridAvailable)
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(x => x.Id == statusLog.AgentId);
                if (agent is not null)
                {
                    var gridStatusNotification = new TelegramNotification
                    {
                        Template = statusLog.IsGridAvailable ? TextConstants.MessageTemplate.GridOnline : TextConstants.MessageTemplate.GridOffline,
                        Values = [StringHelpers.FormatDuration(dateNow - lastAgentStatus.CreatedAt)],
                        IsVerbose = false
                    };
                    _messageQueue.Send(MessageTopics.TelegramNotification, gridStatusNotification);
                }
            }
        }
    }
}
