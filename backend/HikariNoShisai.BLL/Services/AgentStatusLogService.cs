using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
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
            var chart = new StatusLogChart() { Title = MessageTemplate.StatusLogChartTitle };
            var isAnyAgentIdFilter = agentId == default;
            if (endDate == default)
            {
                endDate = DateTimeOffset.UtcNow;
            }

            var previousLog = await _context.AgentStatusLogs
                .Where(x => x.CreatedAt < startDate && (isAnyAgentIdFilter || x.AgentId == agentId))
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
            var logs = await _context.AgentStatusLogs
                .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate && (isAnyAgentIdFilter || x.AgentId == agentId))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            if (logs.Count == 0)
            {
                chart.GridAvailableCount = previousLog?.IsGridAvailable == true ? 100 : 0;
                return chart;
            }

            var totalDuration = (endDate - (previousLog is null ? logs[0].CreatedAt: startDate)).TotalSeconds;
            var availableDuration = 0.0;

            previousLog?.CreatedAt = startDate;

            foreach (var log in logs)
            {
                if (previousLog is not null && previousLog.IsGridAvailable)
                {
                    availableDuration += (log.CreatedAt - previousLog.CreatedAt).TotalSeconds;
                }
                previousLog = log;
            }

            if (previousLog?.IsGridAvailable == true)
            {
                availableDuration += (endDate - previousLog.CreatedAt).TotalSeconds;
            }

            chart.GridAvailableCount = Math.Round(availableDuration / totalDuration * 100, 2);

            return chart;
        }

        private async Task EmitGridNotification(AgentStatusLogRequest statusLog, DateTimeOffset dateNow)
        {
            var lastAgentStatus = await _context.AgentStatusLogs.OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(x => x.AgentId == statusLog.AgentId);
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
