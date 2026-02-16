namespace HikariNoShisai.Common.Models
{
    public class Message<T>
    {
        public required T Data { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Topic { get; set; }
    }
}
