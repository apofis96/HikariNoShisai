using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Models;

namespace HikariNoShisai.Common.Interfaces
{
    public interface IAgentStatusLogService
    {
        Task Create(AgentStatusLogRequest statusLog);
        Task<StatusLogChart> GetDailyGridStatistics(DateTimeOffset endDate, Guid agentId = default);
        Task<List<StatusLogChart>> GetMultipleDailyGridStatistics(DateTimeOffset endDate, DateTimeOffset startDate, Guid agentId = default);
    }
}
