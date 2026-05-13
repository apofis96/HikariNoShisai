namespace HikariNoShisai.Common.Models
{
    public class StatusLogDayCumulative
    {
        public required DateTimeOffset Date { get; set; }
        public StatusLogDayCumulativeData? HeadData { get; set; } = null;

        public void AddPeriod(int periodSeconds, bool isAvailablePeriod)
        {
            if (HeadData is null)
            {
                HeadData = new StatusLogDayCumulativeData { PeriodSeconds = periodSeconds, IsAvailable = isAvailablePeriod };
                return;
            }

            var currentData = HeadData;
            while (currentData.NextData is not null)
            {
                currentData = currentData.NextData;
            }
            currentData.NextData = new StatusLogDayCumulativeData { PeriodSeconds = periodSeconds, IsAvailable = isAvailablePeriod };
        }

        public IEnumerable<StatusLogDayCumulativeData> GetNext()
        {
            var current = HeadData;
            while (current is not null)
            {
                yield return current;
                current = current.NextData;
            }
        }


        public class StatusLogDayCumulativeData
        {
            public int PeriodSeconds  { get; set; } = 0;
            public bool IsAvailable { get; set; } = false;
            public StatusLogDayCumulativeData? NextData { get; set; } = null;
        }
    }
}
