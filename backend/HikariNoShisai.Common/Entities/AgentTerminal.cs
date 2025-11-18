namespace HikariNoShisai.Common.Entities
{
    public class AgentTerminal : BaseEntity
    {
        public Guid AgentId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsActive { get; set; } = false;

        public Agent Agent { get; set; } = null!;
    }
}
