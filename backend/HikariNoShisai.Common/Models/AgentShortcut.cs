namespace HikariNoShisai.Common.Models
{
    public class AgentShortcut
    {
        public required Guid AgentTerminalId { get; set; }
        public required string Name { get; set; }
        public required int RowIndex { get; set; } = 0;
        public required int ColumnIndex { get; set; } = 0;
    }
}
