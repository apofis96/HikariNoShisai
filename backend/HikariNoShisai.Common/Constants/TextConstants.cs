namespace HikariNoShisai.Common.Constants
{
    public static class TextConstants
    {
        public enum MessageTemplate
        {
            GridOnline,
            GridOffline,
            AgentAlert
        }

        private static readonly Dictionary<MessageTemplate, string> _enTemplates = new()
        {
            { MessageTemplate.GridOnline, "🟢 Grid is online {0}.\nWas unavailable for {1}" },
            { MessageTemplate.GridOffline, "🔴 Grid is offline {0}.\nWas available for {1}" },
            { MessageTemplate.AgentAlert, "Alert: Agent '{0}' is offline." }
        };

        private static readonly Dictionary<MessageTemplate, string> _ukTemplates = new()
        {
            { MessageTemplate.GridOnline, "🟢 Електрохарчування відновлено о {0}.\n Електродієта тривала {1} годин" },
            { MessageTemplate.GridOffline, "🔴 Електрохарчування зникло о {0}.\n Електрохарчувались {1} годин" },
            { MessageTemplate.AgentAlert, "Увага: Агент '{0}' офлайн." }
        };

        private static readonly Dictionary<MessageTemplate, string> _ruTemplates = new()
        {
            { MessageTemplate.GridOnline, "🟢 Электропитание восстановлено в: {0}.\n Отсутствовало {1} часов" },
            { MessageTemplate.GridOffline, "🔴 Электропитание отсутствует с {0}\n Наличествовало {1} часов" },
            { MessageTemplate.AgentAlert, "Внимание: Агент '{0}' офлайн." }
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
    }
}
