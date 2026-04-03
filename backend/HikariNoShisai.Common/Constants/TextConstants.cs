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
            NotificationsHeader,
            LanguageHeader,
            OffsetHeader,
            ShortcutHeader,
            ShortcutAddHeader,
            ShortcutRemoveHeader,

            ButtonCancel,
            ButtonNotifications,
            ButtonLanguage,
            ButtonOffset,
            ButtonShortcut,
            ButtonShortcutAdd,
            ButtonShortcutRemove,

            ButtonEnglish,
            ButtonUkrainian,
            ButtonRussian,
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
            { MessageTemplate.NotificationsHeader, "Current Notifications setup is {0}\n Type new value or Cancel" },
            { MessageTemplate.LanguageHeader, "Select Language" },
            { MessageTemplate.OffsetHeader, "Current timezone offset is {0}\n Type new value or Cancel" },
            { MessageTemplate.ShortcutHeader, "Quick Access editing" },
            { MessageTemplate.ShortcutAddHeader, "Send terminal id with shortcut name or Cancel" },
            { MessageTemplate.ShortcutRemoveHeader, "Select shortcut to remove or Cancel" },

            { MessageTemplate.ButtonCancel, "Cancel"  },
            { MessageTemplate.ButtonNotifications, "Notifications" },
            { MessageTemplate.ButtonLanguage, "Language" },
            { MessageTemplate.ButtonOffset, "Offset" },
            { MessageTemplate.ButtonEnglish, "English" },
            { MessageTemplate.ButtonUkrainian, "Українська" },
            { MessageTemplate.ButtonRussian, "Русский" },
            { MessageTemplate.ButtonShortcut, "Quick Access" },
            { MessageTemplate.ButtonShortcutAdd, "Add" },
            { MessageTemplate.ButtonShortcutRemove, "Remove" },
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
            { MessageTemplate.NotificationsHeader, "Поточна настройка сповіщень {0}\n Введіть нове значение або Скасувати" },
            { MessageTemplate.LanguageHeader, "Виберіть мову" },
            { MessageTemplate.OffsetHeader, "Поточне зміщення часового поясу {0}\n Введіть нове значение або Скасувати" },
            { MessageTemplate.ShortcutHeader, "Редагування швидкого доступа" },
            { MessageTemplate.ShortcutAddHeader, "Надішліть id терміналу з ім'ям швидкого доступу або або Скасувати" },
            { MessageTemplate.ShortcutRemoveHeader, "Виберіть швидкий доступ для видалення або Скасувати" },

            { MessageTemplate.ButtonCancel, "Скасувати"  },
            { MessageTemplate.ButtonNotifications, "Сповіщення" },
            { MessageTemplate.ButtonLanguage, "Мова" },
            { MessageTemplate.ButtonOffset, "Зміщення" },
            { MessageTemplate.ButtonShortcut, "Швидкий доступ" },
            { MessageTemplate.ButtonShortcutAdd, "Додати" },
            { MessageTemplate.ButtonShortcutRemove, "Видалити" },
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
            { MessageTemplate.NotificationsHeader, "Текущая настройка уведомлений {0}\n Введите новое значение или Отмена" },
            { MessageTemplate.LanguageHeader, "Выберите язык" },
            { MessageTemplate.OffsetHeader, "Текущее смещение часового пояса {0}\n Введите новое значение или Отмена"  },
            { MessageTemplate.ShortcutHeader, "Редактирование быстрого доступа" },
            { MessageTemplate.ShortcutAddHeader, "Отправьте id терминала с именем быстрого доступа или Отмена" },
            { MessageTemplate.ShortcutRemoveHeader, "Выберите быстрый доступ для удаления или Отмена" },

            { MessageTemplate.ButtonCancel, "Отмена"   },
            { MessageTemplate.ButtonNotifications, "Уведомления" },
            { MessageTemplate.ButtonLanguage, "Язык" },
            { MessageTemplate.ButtonOffset, "Смещение" },
            { MessageTemplate.ButtonShortcut, "Быстрый доступ" },
            { MessageTemplate.ButtonShortcutAdd, "Добавить" },
            { MessageTemplate.ButtonShortcutRemove, "Удалить" },
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
