namespace HikariNoShisai.Common.Models
{
    public class AgentShortcut
    {
        public required Guid AgentId { get; set; }
        public required Guid TerminalId { get; set; }
        public required string Name { get; set; }
        public required int RowIndex { get; set; } = 0;
        public required int ColumnIndex { get; set; } = 0;
    }
}
