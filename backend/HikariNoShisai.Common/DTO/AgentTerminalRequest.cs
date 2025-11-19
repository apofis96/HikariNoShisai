namespace HikariNoShisai.Common.DTO
{
    public class AgentTerminalRequest
    {
        public required Guid AgentId { get; set; }
        public required Guid TerminalId { get; set; }
    }
}
