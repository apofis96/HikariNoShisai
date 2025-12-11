namespace HikariNoShisai.Common.DTO
{
    public class AgentStatusLogRequest
    {
        public required Guid AgentId { get; set; }
        public required bool IsGridAvailable { get; set; }
        public int GridVoltage { get; set; } = -1;
        public int BatteryVoltage { get; set; } = -1;
    }
}
