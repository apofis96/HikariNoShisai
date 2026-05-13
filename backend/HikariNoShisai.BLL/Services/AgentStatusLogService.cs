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

        private async Task<StatusLogChart> GetGridStatistics(List<AgentStatusLog> logs, double totalDuration, DateTimeOffset endDate)
        {
            var chart = new StatusLogChart() { };

            if (logs.Count == 0)
            {
                chart.GridAvailableCount = 0;
                return chart;
            }

            var availableDuration = 0.0;
            AgentStatusLog previousLog = null!;

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

        private async Task<StatusLogDayCumulative> GetGridCumulativeStatistics(List<AgentStatusLog> logs, DateTimeOffset endDate)
        {
            var chart = new StatusLogDayCumulative { Date = endDate.Date };

            if (logs.Count == 0)
                return chart;

            for (var i = 0; i < logs.Count; i++)
            {
                if (i + 1 != logs.Count)
                {
                    var seconds = (logs[i + 1].CreatedAt - logs[i].CreatedAt).TotalSeconds;
                    chart.AddPeriod((int)seconds, logs[i].IsGridAvailable);
                }
                else
                {
                    var seconds = (endDate - logs[i].CreatedAt).TotalSeconds;
                    chart.AddPeriod((int)seconds, logs[i].IsGridAvailable);
                }
            }            

            return chart;
        }

        private async Task<List<AgentStatusLog>> GetAgentStatusLogs(Guid agentId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            List<AgentStatusLog> allLogs = [];
            var isAnyAgentIdFilter = agentId == default;
            var previousLog = await _context.AgentStatusLogs
                .Where(x => x.CreatedAt < startDate && (isAnyAgentIdFilter || x.AgentId == agentId))
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (previousLog is not null)
                allLogs.Add(previousLog);

            var logs = await _context.AgentStatusLogs
                .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate && (isAnyAgentIdFilter || x.AgentId == agentId))
                .OrderBy(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            allLogs.AddRange(logs);

            if(allLogs.Count != 0)
                allLogs[0].CreatedAt = startDate;

            return allLogs;
        }

        public async Task<StatusLogChart> GetDailyGridStatistics(DateTimeOffset endDate, Guid agentId = default)
        {
            var startDate = endDate.AddDays(-1);
            var allLogs = await GetAgentStatusLogs(agentId, startDate, endDate);
            var totalDuration = 0.0;

            if (allLogs.Count != 0)
            {
                totalDuration = (endDate - allLogs[0].CreatedAt).TotalSeconds;
            }

            return await GetGridStatistics(allLogs, totalDuration, endDate);
        }
        public async Task<StatusLogDayCumulative> GetDailyGridCumulativeStatistics(DateTimeOffset endDate, Guid agentId = default)
        {
            var startDate = endDate.AddDays(-1);
            var allLogs = await GetAgentStatusLogs(agentId, startDate, endDate);
 
            return await GetGridCumulativeStatistics(allLogs, endDate);
        }

        public async Task<List<StatusLogChart>> GetMultipleDailyGridStatistics(DateTimeOffset endDate, DateTimeOffset startDate, Guid agentId = default)
        {
            var isAnyAgentIdFilter = agentId == default;
            AgentStatusLog? previousLog = null;
            var allLogs = await GetAgentStatusLogs(agentId, startDate, endDate);

            var result = new List<StatusLogChart>();
            var days = (endDate.Date - startDate.Date).Days;

            for (var i = 0; i < days; i++)
            {
                var dayStart = startDate.AddDays(i);
                var dayEnd = dayStart.AddDays(1);
                var dailyLogs = allLogs.Where(x => x.CreatedAt >= dayStart && x.CreatedAt < dayEnd).ToList();
                if (previousLog is not null)
                    dailyLogs.Insert(0, previousLog);
                if (dailyLogs.Count == 0)
                    continue;

                var totalDuration = (dayEnd - dayStart).TotalSeconds;
                var chart = await GetGridStatistics(dailyLogs, totalDuration, dayEnd);
                chart.Title = dayStart.ToString("yyyy-MM-dd");
                result.Add(chart);

                previousLog = dailyLogs.Last();
                previousLog?.CreatedAt = dayEnd;
            }

            return result;
        }

        public async Task<List<StatusLogDayCumulative>> GetMultipleDailyGridCumulativeStatistics(DateTimeOffset endDate, DateTimeOffset startDate, Guid agentId = default)
        {
            var isAnyAgentIdFilter = agentId == default;
            AgentStatusLog? previousLog = null;
            var allLogs = await GetAgentStatusLogs(agentId, startDate, endDate);

            var result = new List<StatusLogDayCumulative>();
            var days = (endDate.Date - startDate.Date).Days;

            for (var i = 0; i < days; i++)
            {
                var dayStart = startDate.AddDays(i);
                var dayEnd = dayStart.AddDays(1);
                var dailyLogs = allLogs.Where(x => x.CreatedAt >= dayStart && x.CreatedAt < dayEnd).ToList();
                if (previousLog is not null)
                    dailyLogs.Insert(0, previousLog);

                var chart = await GetGridCumulativeStatistics(dailyLogs, dayEnd);
                result.Add(chart);

                previousLog = dailyLogs.Last();
                previousLog?.CreatedAt = dayEnd;
            }

            return result;
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
