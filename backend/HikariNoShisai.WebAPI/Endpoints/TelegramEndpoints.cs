using HikariNoShisai.Common.Configs;
using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
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
            if (msg is null)
                return;
            string response;

            try
            {
                if (msg.Text! == TelegramCommands.Start)
                {
                    await userService.Create(msg.From!.Id, msg.Chat.Id);
                    response = "Welcome to Hikari no Shisai!";
                }
                else
                    response = await telegramService.Handle(msg.Text!);
            } catch (Exception ex)
            {
                response = $"An error occurred: {ex.Message}";
            }

            await bot.SendHtml(msg.Chat, response);
        }
    }
}
