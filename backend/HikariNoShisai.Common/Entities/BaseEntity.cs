namespace HikariNoShisai.Common.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public string CreatedAtFormatted {  get; set; }
        public string UpdatedAtFormatted { get; set; }
    }
}
