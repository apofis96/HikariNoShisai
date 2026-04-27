namespace HikariNoShisai.Common.Models
{
    public class StatusLogChart
    {
        public double GridAvailableCount { get; set; } = 0;
        public double GridUnavailableCount { get => 100.00 - GridAvailableCount; set { } }
        public string Title { get; set; } = string.Empty;
    }
}
