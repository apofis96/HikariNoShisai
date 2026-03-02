using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.BLL.Services
{
    public class TelegramService(
        IAgentService agentService,
        IAgentTerminalService agentTerminal,
        IUserService userService) : ITelegramService
    {
        private readonly IAgentService _agentService = agentService;
        private readonly IAgentTerminalService _agentTerminal = agentTerminal;
        private readonly IUserService _userService = userService;

        public async Task<string> Handle(long userId, string message)
        {
            var userLanguage = await _userService.GetLanguageByUserId(userId);
            if (message.StartsWith('/'))
                return await ParseCommand(message, userLanguage);

            return TextConstants.GetMessageFromTemplate(TextConstants.MessageTemplate.InvalidFormat, userLanguage);
        }

        private Task<string> ParseCommand(string message, string language)
        {
            var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts[0] switch
            {
                TelegramCommands.ShowAll => ShowAllCommand(),
                TelegramCommands.Toggle when parts.Length == 3 => CommandExecute(() => _agentTerminal.ToggleAgentTerminalStatus(Guid.Parse(parts[1]), Guid.Parse(parts[2])), language),
                _ => Task.FromResult(TextConstants.GetMessageFromTemplate(TextConstants.MessageTemplate.UnknownCommand, language))
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

        private async Task<string> CommandExecute(Func<Task> action, string language)
        {
            await action();

            return TextConstants.GetMessageFromTemplate(TextConstants.MessageTemplate.SuccessfulCommand, language);
        }
    }
}
