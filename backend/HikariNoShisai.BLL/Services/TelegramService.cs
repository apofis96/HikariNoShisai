using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using static HikariNoShisai.Common.Constants.TextConstants;

namespace HikariNoShisai.BLL.Services
{
    public class TelegramService(
        IAgentService agentService,
        IAgentTerminalService agentTerminal,
        IUserService userService,
        IMemoryCache memoryCache) : ITelegramService
    {
        private readonly IAgentService _agentService = agentService;
        private readonly IAgentTerminalService _agentTerminal = agentTerminal;
        private readonly IUserService _userService = userService;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private const string CacheKeyPrefix = "telegram_";
        private readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);

        public async Task<string> Handle(long userId, string message)
        {
            var userLanguage = await _userService.GetLanguageByUserId(userId);
            if (message.StartsWith('/'))
                return await ParseCommand(userId, message, userLanguage);

            if (_memoryCache.TryGetValue<TelegramCache>(userId, out var cacheEntry) && cacheEntry is not null)
            {
                return await ParseDialog(cacheEntry.ChatStep, userId, message, userLanguage);
            }

            return GetMessageFromTemplate(MessageTemplate.InvalidFormat, userLanguage);
        }

        private Task<string> ParseDialog(TelegramChatStep chatStep, long userId, string message, string language)
        {
            return chatStep switch
            {
                _ => Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language))
            };
        }

        private Task<string> ParseCommand(long userId, string message, string language)
        {
            var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts[0] switch
            {
                TelegramCommands.ShowAll => ShowAllCommand(),
                TelegramCommands.Settings => SettingsCommand(userId, language),
                TelegramCommands.Toggle when parts.Length == 3 => CommandExecute(() => _agentTerminal.ToggleAgentTerminalStatus(Guid.Parse(parts[1]), Guid.Parse(parts[2])), language),
                _ => Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language))
            };
        }

        private async Task<string> ShowAllCommand()
        {
            var agents = await _agentService.GetAll();
            var result = "Agents:\n";
            foreach (var agent in agents)
            {
                result += $"- {agent.Name} (ID: {agent.Id})\n";
                foreach (var terminal in agent.Terminals)
                {
                    result += $"  - Terminal: {terminal.Name} (ID: {terminal.Id}) - Active: {terminal.IsActive}\n";
                }
            }

            return result;
        }

        private Task<string> SettingsCommand(long userId, string language)
        {
            SetCache(userId, TelegramChatStep.Settings);
            MessageTemplate[] buttons = [MessageTemplate.ButtonNotifications, MessageTemplate.ButtonLanguage, MessageTemplate.ButtonOffset, MessageTemplate.ButtonCancel];
            var settingsMessage = GetMessageFromTemplate(MessageTemplate.SettingsHeader, language);

            return Task.FromResult(ButtonFormatter.AddButtons(settingsMessage, GetMessageFromTemplate(buttons, language)));
        }

        private async Task<string> CommandExecute(Func<Task> action, string language)
        {
            await action();

            return GetMessageFromTemplate(MessageTemplate.SuccessfulCommand, language);
        }

        private void SetCache(long userId, TelegramChatStep chatStep) => _memoryCache.Set($"{CacheKeyPrefix}{userId}", new TelegramCache { ChatStep = chatStep }, Expiration);
    }
}
