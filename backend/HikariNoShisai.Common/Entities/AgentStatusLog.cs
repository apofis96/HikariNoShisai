namespace HikariNoShisai.Common.Entities
{
    public class AgentStatusLog : BaseEntity
    {
        public Guid AgentId { get; set; }
        public bool IsGridAvailable { get; set; } = false;
        public int GridVoltage { get; set; } = 0;
        public int BatteryVoltage { get; set; } = 0;

        public Agent Agent { get; set; } = null!;
    }
}
