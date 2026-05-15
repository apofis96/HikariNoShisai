using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using Microsoft.Extensions.Caching.Memory;
using ScottPlot;
using System.Globalization;
using static HikariNoShisai.Common.Constants.TextConstants;
using static HikariNoShisai.Common.Helpers.StringHelpers;

namespace HikariNoShisai.BLL.Services
{
    public class TelegramService(
        IAgentService agentService,
        IAgentTerminalService agentTerminal,
        IUserService userService,
        IMemoryCache memoryCache,
        ISettingsService settingsService,
        IAgentStatusLogService agentStatusLogService) : ITelegramService
    {
        private readonly IAgentService _agentService = agentService;
        private readonly IAgentTerminalService _agentTerminal = agentTerminal;
        private readonly IUserService _userService = userService;
        private readonly ISettingsService _settingsService = settingsService;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IAgentStatusLogService _agentStatusLogService = agentStatusLogService;
        private const string CacheKeyPrefix = "telegram_";
        private readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);

        public async Task<TelegramHtmlMessage> Handle(long userId, string message)
        {
            var userLanguage = await _userService.GetLanguageByUserId(userId);
            if (message.StartsWith('/'))
                return await ParseCommand(userId, message, userLanguage);

            if (_memoryCache.TryGetValue<TelegramCache>(GetCacheKey(userId), out var cacheEntry) && cacheEntry is not null)
            {
                var command = GetTemplateFromMessage(message, userLanguage);
                if (command == MessageTemplate.ButtonCancel)
                    return new (await FormatResponse(userId, userLanguage, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder], TelegramChatStep.None));

                return await ParseDialog(cacheEntry.ChatStep, userId, message, userLanguage, cacheEntry.Data);
            }
            if (!String.IsNullOrEmpty(message) && Int32.TryParse(message.Split('.', 2)[0], out var index))
                return new (await ParseToggleIndex(userId, index, userLanguage));

            return new (GetMessageFromTemplate(MessageTemplate.InvalidFormat, userLanguage));
        }

        private async Task<TelegramHtmlMessage> ParseDialog(TelegramChatStep chatStep, long userId, string message, string language, object? data)
        {
            return chatStep switch
            {
                TelegramChatStep.Settings => new (await ParseSettingsCommand(userId, message, language)),
                TelegramChatStep.SettingsNotifications => new (await SetSettingsNotificationsCommand(userId, message, language)),
                TelegramChatStep.SettingsOffset => new (await SetSettingsOffsetCommand(userId, message, language)),
                TelegramChatStep.SettingsLanguage => new (await SetLanguageCommand(userId, message, language)),
                TelegramChatStep.SettingsShortcut => new (await ShortcutCommand(userId, message, language)),
                TelegramChatStep.SettingsShortcutAdd => new (await ShortcutAddCommand(userId, message, language)),
                TelegramChatStep.SettingsShortcutRemove => new (await ShortcutRemoveCommand(userId, message, language)),
                TelegramChatStep.Statistics => new (await ParseStatisticsTypeCommand(userId, message, language)),
                TelegramChatStep.StatisticsType => new (await ParseStatisticsTimeframeCommand(userId, message, language, data)),
                TelegramChatStep.StatisticsTimeframe => await ParseStatisticsEndDateCommand(userId, message, language, data),
                _ => new (await Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language)))
            };
        }

        private Task<TelegramHtmlMessage> ParseCommand(long userId, string message, string language)
        {
            var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts[0] switch
            {
                TelegramCommands.Statistics => StatisticsCommand(userId, language),
                TelegramCommands.ShowAll => ShowAllCommand(),
                TelegramCommands.Settings => SettingsCommand(userId, language),
                TelegramCommands.Toggle when parts.Length == 3 => ToggleCommand(userId, parts, language),
                _ => Task.FromResult(new TelegramHtmlMessage { HtmlContent = GetMessageFromTemplate(MessageTemplate.UnknownCommand, language) })
            };
        }
        #region Statistics Dialog
        private async Task<TelegramHtmlMessage> StatisticsCommand(long userId, string language)
        {
            return new TelegramHtmlMessage
            {
                HtmlContent = await FormatResponse(
                    userId,
                    language,
                    MessageTemplate.StatisticsHeader,
                    [MessageTemplate.ButtonStatisticsCumulative, MessageTemplate.ButtonStatisticsTime, MessageTemplate.ButtonCancel],
                    TelegramChatStep.Statistics
                )
            };
        }
        private Task<string> ParseStatisticsTypeCommand(long userId, string message, string language)
        {
            var command = GetTemplateFromMessage(message, language);
            if (command != MessageTemplate.ButtonStatisticsCumulative && command != MessageTemplate.ButtonStatisticsTime)
                return Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language));

            return FormatResponse(
                    userId,
                    language,
                    MessageTemplate.StatisticsTypeHeader,
                    [MessageTemplate.ButtonStatisticsDay, MessageTemplate.ButtonStatisticsWeek, MessageTemplate.ButtonStatisticsMonth, MessageTemplate.ButtonCancel],
                    TelegramChatStep.StatisticsType,
                    data: command);
        }
        private Task<string> ParseStatisticsTimeframeCommand(long userId, string message, string language, object? data)
        {
            if (data is null)
                return Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language));

            var previousCommand = (MessageTemplate)data;
            var command = GetTemplateFromMessage(message, language);

            if (command != MessageTemplate.ButtonStatisticsDay && command != MessageTemplate.ButtonStatisticsWeek && command != MessageTemplate.ButtonStatisticsMonth)
                return Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language));

            var endDateToday = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

            return FormatResponse(
                    userId,
                    language,
                    MessageTemplate.StatisticsTimeframeHeader,
                    chatStep: TelegramChatStep.StatisticsTimeframe,
                    data: (previousCommand, command),
                    rawButtons: [endDateToday]);
        }

        private async Task<TelegramHtmlMessage> ParseStatisticsEndDateCommand(long userId, string message, string language, object? data)
        {
            if (data is null)
                return new(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language));

            (MessageTemplate typeCommand, MessageTemplate timeframeCommand) = ((MessageTemplate, MessageTemplate))data;
            var endDate = DateTimeOffset.MinValue;
            var offset = await _settingsService.GetTimezoneOffset();

            DateTimeOffset.TryParse(message, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out endDate);
            if (endDate ==  DateTimeOffset.MinValue)
                endDate = DateTimeOffset.UtcNow.Date;
            if (timeframeCommand == MessageTemplate.ButtonStatisticsDay)
                endDate = endDate.AddDays(1).AddTicks(-1);
            else
                endDate = endDate.AddTicks(-1);
            endDate = endDate.ToOffset(offset);

            var startDate = timeframeCommand switch
            {
                MessageTemplate.ButtonStatisticsWeek => endDate.AddDays(-7).AddTicks(1),
                MessageTemplate.ButtonStatisticsMonth => endDate.AddMonths(-1).AddTicks(1),
                _ => endDate.AddDays(-1).AddTicks(1),
            };

            Plot plot = new();

            if (timeframeCommand == MessageTemplate.ButtonStatisticsWeek || timeframeCommand == MessageTemplate.ButtonStatisticsMonth)
            {
                var bars = new List<Bar>();
                var ticks = new List<Tick>();

                if (typeCommand == MessageTemplate.ButtonStatisticsCumulative)
                {
                    var statistics = await _agentStatusLogService.GetMultipleDailyGridCumulativeStatistics(endDate, startDate);

                    for (int i = 0; i < statistics.Count; i++)
                    {
                        var valueBase = 0;

                        foreach (var item in statistics[i].GetNext())
                        {
                            bars.Add(new Bar() { Position = i + 1, ValueBase = valueBase, Value = item.PeriodSeconds, FillColor = item.IsAvailable ? Colors.Green : Colors.Red });
                            valueBase += item.PeriodSeconds;
                        }

                        ticks.Add(new Tick(i + 1, statistics[i].Date.ToString("yyyy-MM-dd")));
                    }
                }
                else
                {
                    var statistics = await _agentStatusLogService.GetMultipleDailyGridStatistics(endDate, startDate);

                    for (int i = 0; i < statistics.Count; i++)
                    {
                        bars.Add(new Bar() { Position = i + 1, ValueBase = 0, Value = statistics[i].GridAvailableCount, FillColor = Colors.Green });
                        bars.Add(new Bar() { Position = i + 1, ValueBase = statistics[i].GridAvailableCount, Value = 100.00, FillColor = Colors.Red });

                        ticks.Add(new Tick(i + 1, statistics[i].Title));
                    }
                }
                
                plot.Add.Bars(bars);
                plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual([.. ticks]);
                plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
                plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
                plot.Axes.Bottom.LockSize(60f);
                plot.Axes.Right.LockSize(50f);
                plot.Axes.Left.LockSize(35f);
                plot.Axes.Top.LockSize(15f);
                plot.Axes.Margins(0, 0);
                plot.Grid.MajorLineWidth = 2;
                plot.Grid.MajorLineColor = Colors.White.WithAlpha(.5);
                plot.Grid.IsBeneathPlottables = false;
                plot.Grid.XAxisStyle.IsVisible = false;
            }
            else
            {
                List<PieSlice> slices = [];
                if (typeCommand == MessageTemplate.ButtonStatisticsCumulative)
                {
                    var statistics = await _agentStatusLogService.GetDailyGridCumulativeStatistics(endDate);

                    foreach (var item in statistics.GetNext())
                    {
                        slices.Add(new PieSlice()
                        {
                            Value = item.PeriodSeconds,
                            FillColor = item.IsAvailable ? Colors.Green : Colors.Red,
                            Label = FormatDuration(TimeSpan.FromSeconds(item.PeriodSeconds)),
                            LabelFontSize = 20,
                            LabelBold = true,
                            LabelFontColor = Colors.Black.WithAlpha(.5)
                        });
                    }
                }
                else
                {
                    var statistics = await _agentStatusLogService.GetDailyGridStatistics(endDate);

                    if (statistics.GridAvailableCount > 0.0)
                    {
                        slices.Add(new PieSlice()
                        {
                            Value = statistics.GridAvailableCount,
                            FillColor = Colors.Green,
                            Label = $"{statistics.GridAvailableCount:0}%",
                            LabelFontSize = 20,
                            LabelBold = true,
                            LabelFontColor = Colors.Black.WithAlpha(.5)
                        });
                    }
                    if (statistics.GridUnavailableCount > 0.0)
                    {
                        slices.Add(new PieSlice()
                        {
                            Value = statistics.GridUnavailableCount,
                            FillColor = Colors.Red,
                            Label = $"{statistics.GridUnavailableCount:0}%",
                            LabelFontSize = 20,
                            LabelBold = true,
                            LabelFontColor = Colors.Black.WithAlpha(.5)
                        });
                    }
                }

                plot.Add.Pie(slices);
                plot.Axes.Frameless();
                plot.HideGrid();
            }

            var imageByte = plot.GetImageBytes(512, 512, ImageFormat.Png);
            if (imageByte is null)
                return new TelegramHtmlMessage { HtmlContent = GetMessageFromTemplate(MessageTemplate.InvalidFormat, language) };

            return new TelegramHtmlMessage
            {
                HtmlContent = GetStreamImageTag(0),
                Streams = [new MemoryStream(imageByte)]
            };
        }
        #endregion

        private async Task<TelegramHtmlMessage> ToggleCommand(long userId, string[] parts, string language)
        {
            return new TelegramHtmlMessage
            {
                HtmlContent = await CommandExecute(() => _agentTerminal.ToggleAgentTerminalStatus(Guid.Parse(parts[1]), Guid.Parse(parts[2])), language, userId)
            };
        }

        private async Task<TelegramHtmlMessage> ShowAllCommand()
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

            return new TelegramHtmlMessage { HtmlContent = result };
        }

        private async Task<string> ParseToggleIndex(long userId, int index, string language)
        {
            var shortcuts = await _userService.GetAgentShortcuts(userId);
            var shortcut = shortcuts.ElementAtOrDefault(index - 1);
            if (shortcut is null)
            {
                return await FormatResponse(userId, language, MessageTemplate.InvalidFormat, [MessageTemplate.ButtonShortcutPlaceholder]);
            }

            await _agentTerminal.ToggleAgentTerminalStatus(shortcut.AgentId, shortcut.TerminalId);

            return await FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder]);
        }

        #region Settings Dialog
        private async Task<TelegramHtmlMessage> SettingsCommand(long userId, string language)
        {
            return new TelegramHtmlMessage
            {
                HtmlContent = await FormatResponse(
                    userId,
                    language,
                    MessageTemplate.SettingsHeader,
                    [MessageTemplate.ButtonShortcut, MessageTemplate.ButtonNotifications, MessageTemplate.ButtonLanguage, MessageTemplate.ButtonOffset, MessageTemplate.ButtonCancel],
                    TelegramChatStep.Settings
                )
            };
        }
        private Task<string> ParseSettingsCommand(long userId, string message, string language)
        {
            var command = GetTemplateFromMessage(message, language);
            return command switch
            {
                MessageTemplate.ButtonShortcut => FormatResponse(
                    userId,
                    language,
                    MessageTemplate.ShortcutHeader,
                    [MessageTemplate.ButtonShortcutAdd, MessageTemplate.ButtonShortcutRemove, MessageTemplate.ButtonCancel],
                    TelegramChatStep.SettingsShortcut
                ),
                MessageTemplate.ButtonNotifications => ParseSettingsNotificationsCommand(userId, language),
                MessageTemplate.ButtonLanguage => FormatResponse(
                    userId,
                    language,
                    MessageTemplate.LanguageHeader,
                    [MessageTemplate.ButtonEnglish, MessageTemplate.ButtonUkrainian, MessageTemplate.ButtonRussian, MessageTemplate.ButtonCancel],
                    TelegramChatStep.SettingsLanguage
                ),
                MessageTemplate.ButtonOffset => ParseSettingsOffsetCommand(userId, language),
                _ => Task.FromResult(GetMessageFromTemplate(MessageTemplate.UnknownCommand, language))
            };
        }

        private async Task<string> ParseSettingsNotificationsCommand(long userId, string language)
        {
            var userSettings = await _userService.GetSettings(userId);
            var response = await FormatResponse(
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
                return await FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder], TelegramChatStep.None);
            }

            return await FormatResponse(userId, language, MessageTemplate.InvalidFormat);
        }

        private async Task<string> ParseSettingsOffsetCommand(long userId, string language)
        {
            var offset = await _settingsService.GetTimezoneOffset();
            var response = await FormatResponse(
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
                return await FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder], TelegramChatStep.None);
            }
            return await FormatResponse(userId, language, MessageTemplate.InvalidFormat);
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
                return await FormatResponse(userId, newLanguage, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder], TelegramChatStep.None);
            }
            return await FormatResponse(userId, language, MessageTemplate.InvalidFormat);
        }

        private async Task<string> ShortcutCommand(long userId, string message, string language)
        {
            var command = GetTemplateFromMessage(message, language);

            if (command == MessageTemplate.ButtonShortcutAdd)
            {
                return await FormatResponse(
                    userId,
                    language,
                    MessageTemplate.ShortcutAddHeader,
                    [MessageTemplate.ButtonCancel],
                    TelegramChatStep.SettingsShortcutAdd
                );
            }
            else if (command == MessageTemplate.ButtonShortcutRemove)
            {
                return await ShortcutRemoveCommand(userId, "", language);
            }
            return await FormatResponse(userId, language, MessageTemplate.InvalidFormat);
        }

        private async Task<string> ShortcutAddCommand(long userId, string message, string language)
        {
            string[] parts = message.Split(' ', 2);

            if (!Guid.TryParse(parts[0], out var terminalId) || parts.Length == 1 || String.IsNullOrEmpty(parts[1]))
                return await FormatResponse(userId, language, MessageTemplate.InvalidFormat);
            var agentId = await _agentTerminal.GetAgentIdByTerminalId(terminalId);
            if (agentId == Guid.Empty)
                return await FormatResponse(userId, language, MessageTemplate.InvalidFormat);

            var agentShortcut = new AgentShortcut
            {
                AgentId = agentId,
                TerminalId = terminalId,
                Name = parts[1],
                RowIndex = 0,
                ColumnIndex = 0
            };
            await _userService.AddAgentShortcutToUser(userId, agentShortcut);

            return await FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder], TelegramChatStep.None);
        }

        private async Task<string> ShortcutRemoveCommand(long userId, string message, string language)
        {
            if (!String.IsNullOrEmpty(message) && Int32.TryParse(message.Split('.', 2)[0], out var index))
            {
                await _userService.RemoveAgentShortcut(userId, index - 1);
                return await FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder], TelegramChatStep.None);
            }

            return await FormatResponse(userId, language, MessageTemplate.ShortcutRemoveHeader, [MessageTemplate.ButtonShortcutPlaceholder, MessageTemplate.ButtonCancel]);
        }

        #endregion

        private async Task<string> CommandExecute(Func<Task> action, string language, long userId)
        {
            await action();

            return await FormatResponse(userId, language, MessageTemplate.SuccessfulCommand, [MessageTemplate.ButtonShortcutPlaceholder], TelegramChatStep.None);
        }

        private async Task<string> FormatResponse(
            long userId,
            string language,
            MessageTemplate template,
            MessageTemplate[]? buttons = null,
            TelegramChatStep? chatStep = null,
            object? data = null,
            string[]? rawButtons = null)
        {
            if (chatStep.HasValue)
            {
                if (chatStep.Value == TelegramChatStep.None)
                {
                    ClearCache(userId);
                }
                else
                {
                    SetCache(userId, chatStep.Value, data);
                }
            }
            buttons ??= [];
            string[] formattedButtons = [];

            var shortcutPlaceholder = buttons.IndexOf(MessageTemplate.ButtonShortcutPlaceholder);

            if (shortcutPlaceholder != -1)
            {
                var shortcuts = await _userService.GetAgentShortcuts(userId);
                formattedButtons = [.. shortcuts.Select((s, i) => $"{i + 1}. {s.Name}")];
                buttons = [.. buttons.AsSpan(0, shortcutPlaceholder), .. buttons.AsSpan(shortcutPlaceholder + 1)];
            }

            formattedButtons = [.. formattedButtons, .. GetMessageFromTemplate(buttons, language)];
            
            if (rawButtons is not null)
            {
                formattedButtons = [.. formattedButtons, .. rawButtons];
            }

            return ButtonFormatter.AddButtons(GetMessageFromTemplate(template, language), formattedButtons);
        }
        private string GetCacheKey(long userId) => $"{CacheKeyPrefix}{userId}";
        private void SetCache(long userId, TelegramChatStep chatStep, object? data = null) => _memoryCache.Set(GetCacheKey(userId), new TelegramCache { ChatStep = chatStep, Data = data }, Expiration);
        private void ClearCache(long userId) => _memoryCache.Remove($"{CacheKeyPrefix}{userId}");
    }
}
