namespace HikariNoShisai.Common.Constants
{
    public static class TextConstants
    {
        public enum MessageTemplate
        {
            GridOnline,
            GridOffline,
            AgentAlert,
            WelcomeMessage,
            InvalidFormat,
            UnknownCommand,
            SuccessfulCommand,
            SettingsHeader,

            ButtonCancel,
            ButtonNotifications,
            ButtonLanguage,
            ButtonOffset,
        }

        private static readonly Dictionary<MessageTemplate, string> _enTemplates = new()
        {
            { MessageTemplate.GridOnline, "🟢 Grid is online {0}.\nWas unavailable for {1}" },
            { MessageTemplate.GridOffline, "🔴 Grid is offline {0}.\nWas available for {1}" },
            { MessageTemplate.AgentAlert, "Alert: Agent '{0}' is offline." },
            { MessageTemplate.WelcomeMessage, "Welcome to Hikari no Shisai!" },
            { MessageTemplate.InvalidFormat, "Invalid command format." },
            { MessageTemplate.UnknownCommand, "Unknown command or invalid parameters." },
            { MessageTemplate.SuccessfulCommand, "Command executed successfully." },
            { MessageTemplate.SettingsHeader, "Choose Settings"},

            { MessageTemplate.ButtonCancel, "Cancel"  },
            { MessageTemplate.ButtonNotifications, "Notifications" },
            { MessageTemplate.ButtonLanguage, "Language" },
            { MessageTemplate.ButtonOffset, "Offset" },
        };

        private static readonly Dictionary<MessageTemplate, string> _ukTemplates = new()
        {
            { MessageTemplate.GridOnline, "🟢 Електрохарчування відновлено о {0}.\n Електродієта тривала {1} годин" },
            { MessageTemplate.GridOffline, "🔴 Електрохарчування зникло о {0}.\n Електрохарчувались {1} годин" },
            { MessageTemplate.AgentAlert, "Увага: Агент '{0}' офлайн." },
            { MessageTemplate.WelcomeMessage, "Ласкаво просимо до Hikari no Shisai!" },
            { MessageTemplate.InvalidFormat, "Невірний формат команди." },
            { MessageTemplate.UnknownCommand, "Невідома команда або невірні параметры." },
            { MessageTemplate.SuccessfulCommand, "Команда успішно виконана." },
            { MessageTemplate.SettingsHeader, "Виберіть налаштування" },

            { MessageTemplate.ButtonCancel, "Скасувати"  },
            { MessageTemplate.ButtonNotifications, "Сповіщення" },
            { MessageTemplate.ButtonLanguage, "Мова" },
            { MessageTemplate.ButtonOffset, "Зміщення" },
        };

        private static readonly Dictionary<MessageTemplate, string> _ruTemplates = new()
        {
            { MessageTemplate.GridOnline, "🟢 Электропитание восстановлено в: {0}.\n Отсутствовало {1} часов" },
            { MessageTemplate.GridOffline, "🔴 Электропитание отсутствует с {0}\n Наличествовало {1} часов" },
            { MessageTemplate.AgentAlert, "Внимание: Агент '{0}' офлайн." },
            { MessageTemplate.WelcomeMessage, "Добро пожаловать в Hikari no Shisai!" },
            { MessageTemplate.InvalidFormat, "Неверный формат команды." },
            { MessageTemplate.UnknownCommand, "Неизвестная команда или неверные параметры." },
            { MessageTemplate.SuccessfulCommand, "Команда успешно выполнена." },
            { MessageTemplate.SettingsHeader, "Выберите настройки" },

            { MessageTemplate.ButtonCancel, "Отмена"   },
            { MessageTemplate.ButtonNotifications, "Уведомления" },
            { MessageTemplate.ButtonLanguage, "Язык" },
            { MessageTemplate.ButtonOffset, "Смещение" },
        };

        public static string GetMessageFromTemplate(MessageTemplate template, string languageCode)
        {
            var nonDefaultLookup = languageCode switch
            {
                LanguageCodes.Ukrainian => _ukTemplates.GetValueOrDefault(template),
                LanguageCodes.Russian => _ruTemplates.GetValueOrDefault(template),
                _ => null,
            };

            if (nonDefaultLookup is not null)
                return nonDefaultLookup;

            return _enTemplates.GetValueOrDefault(template) ?? "";
        }

        public static string[] GetMessageFromTemplate(MessageTemplate[] templates, string languageCode)
        {
            return templates.Select(MessageTemplate => GetMessageFromTemplate(MessageTemplate, languageCode)).ToArray();
        }

        public static MessageTemplate GetTemplateFromMessage(string message, string languageCode)
        {
            var templates = languageCode switch
            {
                LanguageCodes.Ukrainian => _ukTemplates,
                LanguageCodes.Russian => _ruTemplates,
                _ => _enTemplates,
            };

            return templates.FirstOrDefault(x => x.Value == message).Key;
        }
    }
}
