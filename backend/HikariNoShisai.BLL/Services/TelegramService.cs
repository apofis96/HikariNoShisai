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
        IMemoryCache memoryCache,
        ISettingsService settingsService) : ITelegramService
    {
        private readonly IAgentService _agentService = agentService;
        private readonly IAgentTerminalService _agentTerminal = agentTerminal;
        private readonly IUserService _userService = userService;
        private readonly ISettingsService _settingsService = settingsService;
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
                var command = GetTemplateFromMessage(message, userLanguage);
                if (command == MessageTemplate.ButtonCancel)
                    return FormatResponse(userId, userLanguage, MessageTemplate.SuccessfulCommand, null, TelegramChatStep.None);

                return await ParseDialog(cacheEntry.ChatStep, userId, message, userLanguage);
            }

            return GetMessageFromTemplate(MessageTemplate.InvalidFormat, userLanguage);
        }

        private Task<string> ParseDialog(TelegramChatStep chatStep, long userId, string message, string language)
        {
            return chatStep switch
            {
                TelegramChatStep.Settings => ParseSettingsCommand(userId, message, language),
                TelegramChatStep.SettingsNotifications => SetSettingsNotificationsCommand(userId, message, language),
                TelegramChatStep.SettingsOffset => SetSettingsOffsetCommand(userId, message, language),
                TelegramChatStep.SettingsLanguage => SetLanguageCommand(userId, message, language),
                _ => Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language))
            };
        }

        private Task<string> ParseCommand(long userId, string message, string language)
        {
            var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts[0] switch
            {
                TelegramCommands.ShowAll => ShowAllCommand(),
                TelegramCommands.Settings => Task.FromResult(FormatResponse(
                    userId,
                    language,
                    MessageTemplate.SettingsHeader,
                    [MessageTemplate.ButtonNotifications, MessageTemplate.ButtonLanguage, MessageTemplate.ButtonOffset, MessageTemplate.ButtonCancel],
                    TelegramChatStep.Settings   
                )),
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

        #region Settings Dialog
        private Task<string> ParseSettingsCommand(long userId, string message, string language)
        {
            var command = GetTemplateFromMessage(message, language);
            return command switch
            {
                MessageTemplate.ButtonNotifications => ParseSettingsNotificationsCommand(userId, language),
                MessageTemplate.ButtonLanguage => Task.FromResult(FormatResponse(
                    userId,
                    language,
                    MessageTemplate.LanguageHeader,
                    [MessageTemplate.ButtonEnglish, MessageTemplate.ButtonUkrainian, MessageTemplate.ButtonRussian, MessageTemplate.ButtonCancel],
                    TelegramChatStep.SettingsLanguage
                )),
                MessageTemplate.ButtonOffset => ParseSettingsOffsetCommand(userId, language),
                _ => Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language))
            };
        }

        private async Task<string> ParseSettingsNotificationsCommand(long userId, string language)
        {
            var userSettings = await _userService.GetSettings(userId);
            var response = FormatResponse(
                userId,
                language,
                MessageTemplate.NotificationsHeader,
                [MessageTemplate.ButtonCancel],
                TelegramChatStep.SettingsNotifications
            );

            return StringHelpers.ReplacePlaceholder(response, ((long)userSettings).ToString());
        }

        private async Task<string> SetSettingsNotificationsCommand(long userId, string message, string language)
        {
            if (Enum.TryParse<UserSettings>(message, true, out var newSetting))
            {
                await _userService.SetSettings(userId, newSetting);
                return FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, null, TelegramChatStep.None);
            }

            return FormatResponse(userId, language, MessageTemplate.InvalidFormat);
        }

        private async Task<string> ParseSettingsOffsetCommand(long userId, string language)
        {
            var offset = await _settingsService.GetTimezoneOffset();
            var response = FormatResponse(
                userId,
                language,
                MessageTemplate.OffsetHeader,
                [MessageTemplate.ButtonCancel],
                TelegramChatStep.SettingsOffset
            );

            return StringHelpers.ReplacePlaceholder(response, offset.TotalMinutes.ToString());
        }

        private async Task<string> SetSettingsOffsetCommand(long userId, string message, string language)
        {
            if (int.TryParse(message, out var newOffset) && newOffset >= (-12 * 60) && newOffset <= (14 * 60))
            {
                await _settingsService.SetTimezoneOffset(newOffset);
                return FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, null, TelegramChatStep.None);
            }
            return FormatResponse(userId, language, MessageTemplate.InvalidFormat);
        }

        private async Task<string> SetLanguageCommand(long userId, string message, string language)
        {
            var command = GetTemplateFromMessage(message, language);
            var newLanguage = command switch
            {
                MessageTemplate.ButtonEnglish => LanguageCodes.English,
                MessageTemplate.ButtonUkrainian => LanguageCodes.Ukrainian,
                MessageTemplate.ButtonRussian => LanguageCodes.Russian,
                _ => LanguageCodes.English
            };
            if (newLanguage is not null)
            {
                await _userService.SetLanguage(userId, newLanguage);
                return FormatResponse(userId, newLanguage, MessageTemplate.SuccessfulCommand, null, TelegramChatStep.None);
            }
            return FormatResponse(userId, language, MessageTemplate.InvalidFormat);
        }
        #endregion

        private async Task<string> CommandExecute(Func<Task> action, string language)
        {
            await action();

            return GetMessageFromTemplate(MessageTemplate.SuccessfulCommand, language);
        }

        private string FormatResponse(long userId, string language, MessageTemplate template, MessageTemplate[]? buttons = null, TelegramChatStep? chatStep = null)
        {
            if (chatStep.HasValue)
            {
                if (chatStep.Value == TelegramChatStep.None)
                {
                    ClearCache(userId);
                }
                else
                {
                    SetCache(userId, chatStep.Value);
                }
            }
            buttons ??= [];

            return ButtonFormatter.AddButtons(
                GetMessageFromTemplate(template, language),
                GetMessageFromTemplate(buttons, language));
        }
        private void SetCache(long userId, TelegramChatStep chatStep) => _memoryCache.Set($"{CacheKeyPrefix}{userId}", new TelegramCache { ChatStep = chatStep }, Expiration);
        private void ClearCache(long userId) => _memoryCache.Remove($"{CacheKeyPrefix}{userId}");
    }
}
