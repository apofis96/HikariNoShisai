using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class TelegramEndpoints
    {
        public static void MapTelegramEndpoints(this WebApplication app)
        {

            var botApi = app.MapGroup("/bot");
            botApi.MapGet("/setWebhook", async (TelegramBotClient bot, IConfiguration config) => {
                var webhookUrl = config["Telegram:Url"]!;
                await bot.SetWebhook(webhookUrl); return $"Webhook set to {webhookUrl}";
            });
            botApi.MapPost("/", OnUpdate);
        }

        static async void OnUpdate(TelegramBotClient bot, Update update)
        {
            if (update.Message is null) return;         // we want only updates about new Message
            if (update.Message.Text is null) return;    // we want only updates about new Text Message
            var msg = update.Message;
            Console.WriteLine($"Received message '{msg.Text}' in {msg.Chat}");
            // let's echo back received text in the chat
            await bot.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");
        }
    }
}
