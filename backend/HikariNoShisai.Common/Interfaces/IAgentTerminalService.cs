namespace HikariNoShisai.Common.Interfaces
{
    public interface IAgentTerminalService
    {
        Task<sbyte> GetAgentTerminalStatus(Guid agentId, Guid terminalId);
    }
}
