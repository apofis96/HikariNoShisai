using HikariNoShisai.Common.Entities;

namespace HikariNoShisai.Common.Interfaces
{
    public interface IAgentService
    {
        Task<IEnumerable<Agent>> GetAll();
    }
}
