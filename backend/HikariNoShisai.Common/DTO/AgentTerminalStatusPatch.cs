namespace HikariNoShisai.Common.DTO
{
    public class AgentTerminalStatusPatch
    {
        public required Guid AgentId { get; set; }
        public required Guid TerminalId { get; set; }
        public required bool IsActive { get; set; }
    }
}
