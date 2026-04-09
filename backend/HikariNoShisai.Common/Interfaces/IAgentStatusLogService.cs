using HikariNoShisai.Common.DTO;

namespace HikariNoShisai.Common.Interfaces
{
    public interface IAgentStatusLogService
    {
        Task Create(AgentStatusLogRequest statusLog);
        Task<byte[]> GetGridStatistics(Guid agentId);
    }
}
