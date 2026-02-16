namespace HikariNoShisai.Common.Interfaces
{
    public interface IAgentWatchdog
    {
        void Update(Guid id);
        IEnumerable<Guid> GetExpired(TimeSpan interval);
    }
}
