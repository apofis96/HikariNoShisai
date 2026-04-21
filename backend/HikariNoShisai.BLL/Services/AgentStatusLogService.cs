using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using HikariNoShisai.DAL;
using static HikariNoShisai.Common.Constants.TextConstants;

namespace HikariNoShisai.BLL.Services
{
    public class AgentStatusLogService(HikariNoShisaiContext context, IMessageQueue messageQueue, ISettingsService settingsService) : IAgentStatusLogService
    {
        private readonly HikariNoShisaiContext _context = context;
        private readonly IMessageQueue _messageQueue = messageQueue;
        private readonly ISettingsService _settingsService = settingsService;

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

        public async Task<StatusLogChart> GetGridStatistics(DateTimeOffset startDate, DateTimeOffset endDate = default, Guid agentId = default)
        {
            if (endDate == default)
            {
                endDate = DateTimeOffset.UtcNow;
            }

            return new StatusLogChart() { Title = MessageTemplate.StatusLogChartTitle };

        }

        private async Task EmitGridNotification(AgentStatusLogRequest statusLog, DateTimeOffset dateNow)
        {
            var lastAgentStatus = _context.AgentStatusLogs.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x => x.AgentId == statusLog.AgentId);
            if (lastAgentStatus is not null && lastAgentStatus.IsGridAvailable != statusLog.IsGridAvailable)
            {
                var offset = await _settingsService.GetTimezoneOffset();
                var gridStatusNotification = new TelegramNotification
                {
                    Template = statusLog.IsGridAvailable ? TextConstants.MessageTemplate.GridOnline : TextConstants.MessageTemplate.GridOffline,
                    Values = [dateNow.ToOffset(offset).ToString(), StringHelpers.FormatDuration(dateNow - lastAgentStatus.CreatedAt)],
                    IsVerbose = false
                };
                _messageQueue.Send(MessageTopics.TelegramNotification, gridStatusNotification);
            }
        }
    }
}
