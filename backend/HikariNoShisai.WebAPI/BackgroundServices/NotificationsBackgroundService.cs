using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Helpers;
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

                    var users = await userService.GetUsers(UserSettings.NotificationsEnabled);

                    foreach (var user in users)
                    {
                        var isUserVerbose = ((UserSettings)user.Settings).HasFlag(UserSettings.VerboseNotifications);
                        foreach (var notification in notifications)
                        {
                            if (notification.IsVerbose && !isUserVerbose)
                            {
                                continue;
                            }

                            var message = TextConstants.GetMessageFromTemplate(notification.Template, user.Language);

                            await telegramClient.SendHtml(user.ChatId, StringHelpers.ReplacePlaceholder(message, notification.Values));
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