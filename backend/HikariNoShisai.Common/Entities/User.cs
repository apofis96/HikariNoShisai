namespace HikariNoShisai.Common.Entities
{
    public class User : BaseEntity
    {
        public long UserId { get; set; }
        public long ChatId { get; set; }
        public long Settings { get; set; }
        public required string Language { get; set; }
    }
}
