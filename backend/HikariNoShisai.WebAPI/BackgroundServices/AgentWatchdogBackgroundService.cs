using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;

namespace HikariNoShisai.WebAPI.BackgroundServices
{
    public class AgentWatchdogBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AgentWatchdogBackgroundService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<AgentWatchdogBackgroundService> _logger = logger;
        private readonly TimeSpan _agentTimeout = TimeSpan.FromMinutes(3);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var agentService = scope.ServiceProvider.GetRequiredService<IAgentService>();
                    var messageQueue = scope.ServiceProvider.GetRequiredService<IMessageQueue>();
                    var agentWatchdog = scope.ServiceProvider.GetRequiredService<IAgentWatchdog>();

                    var expiredAgentIds = agentWatchdog.GetExpired(_agentTimeout);

                    foreach (var agentId in expiredAgentIds)
                    {
                        var name = await agentService.GetNameById(agentId);
                        messageQueue.Send(
                            MessageTopics.TelegramNotification,
                            new TelegramNotification {
                                Message = string.Format(TextConstants.AgentOfflineTemplate, name),
                                IsVerbose = true
                            });
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
