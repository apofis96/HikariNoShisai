using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using Telegram.Bot;
using Telegram.Bot.Extensions;

namespace HikariNoShisai.WebAPI.BackgroundServices
{
    public class NotificationsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationsBackgroundService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<NotificationsBackgroundService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    var telegramClient = scope.ServiceProvider.GetRequiredService<TelegramBotClient>();
                    var messageQueue = scope.ServiceProvider.GetRequiredService<IMessageQueue>();

                    var notifications = messageQueue.ReciveAll<TelegramNotification>(MessageTopics.TelegramNotification)?.Select(x => x.Data);
                    if (notifications is null || !notifications.Any())
                    {
                        continue;
                    }

                    var verboseNotifications = notifications.Where(n => n.IsVerbose);
                    var nonVerboseNotifications = notifications.Where(n => !n.IsVerbose);

                    if (verboseNotifications.Any())
                    {
                        var chatIds = await userService.GetChatIds(UserSettings.VerboseNotifications | UserSettings.NotificationsEnabled);

                        foreach (var notification in verboseNotifications)
                        {
                            foreach (var chatId in chatIds)
                            {
                                await telegramClient.SendHtml(chatId, notification.Message);
                            }
                        }
                    }

                    if (nonVerboseNotifications.Any())
                    {
                        var chatIds = await userService.GetChatIds(UserSettings.NotificationsEnabled);
                        foreach (var notification in nonVerboseNotifications)
                        {
                            foreach (var chatId in chatIds)
                            {
                                await telegramClient.SendHtml(chatId, notification.Message);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while running background job");
                }
            }
        }
    }
}