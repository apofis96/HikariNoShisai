using HikariNoShisai.Common.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class TelegramEndpoints
    {
        public static void MapTelegramEndpoints(this WebApplication app)
        {
            const string route = "/bot";

            var botApi = app.MapGroup(route);
            botApi.MapGet("/setWebhook", async (TelegramBotClient bot, IOptions<TelegramConfig> config) => {
                await bot.SetWebhook(config.Value.Url + route); return $"Webhook set to {config.Value.Url + route}";
            });
            botApi.MapPost("/", OnUpdate).AddEndpointFilter(async (context, next) =>
            {
                var config = context.HttpContext.RequestServices.GetRequiredService<IOptions<TelegramConfig>>().Value;
                var updateDto = context.GetArgument<Update>(1);

                if (updateDto?.Message?.Text is null)
                    return Results.Ok();
                if (updateDto.Message.From?.Id is null || !config.AllowedUsers.Contains(updateDto.Message.From.Id))
                    return Results.Ok();

                return await next(context);
            });
        }

        static async void OnUpdate(TelegramBotClient bot, Update update)
        {
            if (update.Message is null) return;         // we want only updates about new Message
            if (update.Message.Text is null) return;    // we want only updates about new Text Message
            var msg = update.Message;
            Console.WriteLine($"Received message '{msg.Text}' in {msg.Chat}");
            // let's echo back received text in the chat
            await bot.SendMessage(
                msg.Chat,
                $"{msg.From} said: {msg.Text}",
                replyMarkup: new string[][]
                {
                    ["Help me"],
                    ["Call me ☎️", "Write me ✉️"],
                    ["😊", "😂", "❤️", "👍", "🎉"],
                    ["Custom Keyboard Markup"],
                    ["Telegram Bot API is awesome!"],
                    ["Inline Keyboard Markup"],
                    ["Telegram Bots are super cool!"],
                    ["Visit Telegram", "https://telegram.org"]
                });
        }
    }
}
