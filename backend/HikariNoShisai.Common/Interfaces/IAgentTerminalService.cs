namespace HikariNoShisai.Common.Interfaces
{
    public interface IAgentTerminalService
    {
        Task<sbyte> GetAgentTerminalStatus(Guid agentId, Guid terminalId);
        Task SetAgentTerminalStatus(Guid agentId, Guid terminalId, bool isActive);
        Task ToggleAgentTerminalStatus(Guid agentId, Guid terminalId);
    }
}
