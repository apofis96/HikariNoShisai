namespace HikariNoShisai.Common.Entities
{
    public class Agent : BaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;

        public ICollection<AgentTerminal> Terminals { get; set; } = new List<AgentTerminal>();
        public ICollection<AgentStatusLog> StatusLogs { get; set; } = new List<AgentStatusLog>();
    }
}
