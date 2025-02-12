using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Examples.Echo
{
    public static class Program
    {
        private static TelegramBotClient Bot;
        private static string tooken = "";
        private static string baseUrl = "";
        private static long[] allowedIds = new long[] {};
        private static readonly HttpClient httpClient = new();

        private static bool mainOn = false;
        private static bool subOn = false;
        private static bool hddOn = false;

        public static async Task Main()
        {
            httpClient.BaseAddress = new Uri(baseUrl);
            Bot = new TelegramBotClient(tooken);

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            Bot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");

            for (;;)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                    break;
            }
            // Send cancellation request to stop bot
            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            if (!allowedIds.Contains(message.Chat.Id))
            {
                await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "User not allowed", replyMarkup: new ReplyKeyboardRemove());
                return;
            }

            switch (message.Text)
            {
                case "Toggle sub":
                    await ToogleSub(message);
                    return;
                case "Toggle main":
                    await ToogleMain(message);
                    return;
                case "Press Button":
                    await PressButton(message);
                    return;
                case "SSD":
                case "HDD":
                    await PressSata(message, false);
                    return;
                case "Yes":
                    await PressSata(message, true);
                    return;
                default:
                    await SendReplyKeyboard(message, "Chose");
                    return;
            }
        }

        static async Task<Message> SendReplyKeyboard(Message message, string msg)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new KeyboardButton[][]
                {
                        new KeyboardButton[] { "Toggle sub", "Toggle main" },
                        new KeyboardButton[] { "Press Button" },
                        new KeyboardButton[] { hddOn ? "SSD" : "HDD" },
                },
                resizeKeyboard: true
            );

            return await Bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                  text: msg,
                                                  replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> ToogleSub(Message message)
        {
            HttpResponseMessage resp;
            if (subOn)
                resp = await httpClient.GetAsync("led_sub_off");
            else
                resp = await httpClient.GetAsync("led_sub_on");

            if (resp.IsSuccessStatusCode)
            {
                subOn = !subOn;
                return await SendReplyKeyboard(message, "Done");
            }
            return await SendReplyKeyboard(message, "Error");
        }

        static async Task<Message> ToogleMain(Message message)
        {
            HttpResponseMessage resp;
            if (mainOn)
                resp = await httpClient.GetAsync("led_main_off");
            else
                resp = await httpClient.GetAsync("led_main_on");

            if (resp.IsSuccessStatusCode)
            {
                mainOn = !mainOn;
                return await SendReplyKeyboard(message, "Done");
            }
            return await SendReplyKeyboard(message, "Error");
        }

        static async Task<Message> PressButton(Message message)
        {
            HttpResponseMessage resp;

            resp = await httpClient.GetAsync("button_comp");

            if (resp.IsSuccessStatusCode)
            {
                return await SendReplyKeyboard(message, "Done");
            }
            return await SendReplyKeyboard(message, "Error");
        }

        static async Task<Message> PressSata(Message message, bool isConfirmed)
        {
            if (!isConfirmed)
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    new KeyboardButton[][]
                    {
                            new KeyboardButton[] { "Yes", "No" },
                    },
                    resizeKeyboard: true
                );

                return await Bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Confirm",
                                                      replyMarkup: replyKeyboardMarkup);
            }
            HttpResponseMessage resp;

            if (hddOn)
                resp = await httpClient.GetAsync("ssd");
            else
                resp = await httpClient.GetAsync("hdd");

            if (resp.IsSuccessStatusCode)
            {
                hddOn = !hddOn;
                return await SendReplyKeyboard(message, "Done");
            }
            return await SendReplyKeyboard(message, "Error");
        }

        private static Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}