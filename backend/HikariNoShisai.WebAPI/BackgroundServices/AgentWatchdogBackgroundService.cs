using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;

namespace HikariNoShisai.WebAPI.BackgroundServices
{
    public class AgentWatchdogBackgroundService(
        IAgentWatchdog agentWatchdog,
        IMessageQueue messageQueue,
        IAgentService agentService,
        ILogger<AgentWatchdogBackgroundService> logger) : BackgroundService
    {
        private readonly IMessageQueue _messageQueue = messageQueue;
        private readonly IAgentWatchdog _agentWatchdog = agentWatchdog;
        private readonly IAgentService _agentService = agentService;
        private readonly ILogger<AgentWatchdogBackgroundService> _logger = logger;
        private readonly TimeSpan _agentTimeout = TimeSpan.FromMinutes(3);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var expiredAgentIds = _agentWatchdog.GetExpired(_agentTimeout);

                    foreach (var agentId in expiredAgentIds)
                    {
                        var name = await _agentService.GetNameById(agentId);
                        _messageQueue.Send(MessageTopics.TelegramNotification, new TelegramNotification { Message = string.Format(TextConstants.AgentOfflineTemplate, name)});
                    }
                }
                catch (OperationCanceledException)
                {}
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while running background job");
                }
            }
        }
    }
}
