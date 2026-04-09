using HikariNoShisai.Common.Configs;
using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class TelegramEndpoints
    {
        public static void MapTelegramEndpoints(this WebApplication app)
        {
            const string route = "/bot";

            var botApi = app.MapGroup(route);
            botApi.MapGet("/setWebhook", async (TelegramBotClient bot, IOptions<TelegramConfig> config) =>
            {
                await bot.SetWebhook(config.Value.Url + route, secretToken: config.Value.Secret);
                await bot.SetMyCommands(
                [
                    new BotCommand { Command = TelegramCommands.Start, Description = "Start the bot" },
                    new BotCommand { Command = TelegramCommands.ShowAll, Description = "Show all terminals" },
                    new BotCommand { Command = TelegramCommands.Toggle, Description = "Toggle [terminal]" },
                    new BotCommand { Command = TelegramCommands.Settings, Description = "Show settings" },
                    new BotCommand { Command = TelegramCommands.Statistics, Description = "Show statistics [days], default 1" },
                ]);
                return $"Webhook set to {config.Value.Url + route}";
            });
            botApi.MapPost("/", OnUpdate).AddEndpointFilter(async (context, next) =>
            {
                var headers = context.HttpContext.Request.Headers;
                if (!headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secret))
                    return Results.BadRequest();

                var config = context.HttpContext.RequestServices.GetRequiredService<IOptions<TelegramConfig>>().Value;
                if (secret != config.Secret)
                    return Results.BadRequest();

                var updateDto = context.GetArgument<Update>(1);

                if (updateDto?.Message?.Text is null)
                    return Results.Ok();

                if (updateDto.Message.From?.Id is null || !config.AllowedUsers.Contains(updateDto.Message.From.Id))
                    return Results.Ok();

                return await next(context);
            });
        }

        static async Task OnUpdate(TelegramBotClient bot, Update update, ITelegramService telegramService, IUserService userService)
        {
            var msg = update.Message;
            if (msg is null || msg.Text is null || msg.From is null)
                return;

            var userId = msg.From.Id;
            TelegramHtmlMessage response;

            try
            {
                if (msg.Text == TelegramCommands.Start)
                {
                    var userLanguage = msg.From.LanguageCode ?? LanguageCodes.English;
                    await userService.Create(userId, msg.Chat.Id, userLanguage);
                    response = new() { HtmlContent = TextConstants.GetMessageFromTemplate(TextConstants.MessageTemplate.WelcomeMessage, userLanguage) };
                }
                else
                {
                    response = await telegramService.Handle(userId, msg.Text);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var isVerbose = await userService.CheckUserSettings(userId, UserSettings.VerboseNotifications);
                    response = new() { HtmlContent = "An error occurred" };
                    if (isVerbose)
                        response.HtmlContent += $": {ex.Message}";
                }
                catch (Exception exx)
                {
                    response = new()
                    {
                        HtmlContent = $"An error occurred while checking user settings: {exx.Message}\n\nOriginal error {ex.Message}"
                    };
                }
            }

            await bot.SendHtml(msg.Chat, response.HtmlContent, streams: response.Streams);
        }
    }
}
