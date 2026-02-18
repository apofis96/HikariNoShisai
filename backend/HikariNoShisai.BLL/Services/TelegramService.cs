using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.BLL.Services
{
    public class TelegramService(
        IAgentService agentService,
        IAgentTerminalService agentTerminal) : ITelegramService
    {
        private readonly IAgentService _agentService = agentService;
        private readonly IAgentTerminalService _agentTerminal = agentTerminal;

        public async Task<string> Handle(string message)
        {
            if (message.StartsWith('/'))
                return await ParseCommand(message[1..]);

            return "Invalid command format.";
        }

        private Task<string> ParseCommand(string message)
        {
            var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts[0] switch
            {
                TelegramCommands.ShowAll => ShowAllCommand(),
                TelegramCommands.Toggle when parts.Length == 3 => CommandExecute(() => _agentTerminal.ToggleAgentTerminalStatus(Guid.Parse(parts[1]), Guid.Parse(parts[2]))),
                _ => Task.FromResult("Unknown command or invalid parameters.")
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

        private async Task<string> CommandExecute(Func<Task> action)
        {
            await action();

            return "Command executed successfully.";
        }
    }
}
