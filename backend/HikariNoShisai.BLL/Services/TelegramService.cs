using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.BLL.Services
{
    public class TelegramService(IAgentService agentService) : ITelegramService
    {
        private readonly IAgentService _agentService = agentService;

        public async Task<string> Handle(string message)
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
    }
}
