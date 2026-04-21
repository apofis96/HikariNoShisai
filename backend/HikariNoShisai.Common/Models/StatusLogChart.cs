using static HikariNoShisai.Common.Constants.TextConstants;

namespace HikariNoShisai.Common.Models
{
    public class StatusLogChart
    {
        public required MessageTemplate Title { get; set; }
        public double GridAvailableCount { get; set; } = 0;
        public double GridUnavailableCount { get => 100.00 - GridAvailableCount; set { } }
    }
}
