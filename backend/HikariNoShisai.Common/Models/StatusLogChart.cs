namespace HikariNoShisai.Common.Models
{
    public class StatusLogChart
    {
        public required string Title { get; set; }
        public double GridAvailableCount { get; set; } = 0;
        public double GridUnavailableCount { get => 100.00 - GridAvailableCount; set { } }
    }
}
