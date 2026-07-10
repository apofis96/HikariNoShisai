namespace HikariNoShisai.Common.Models
{
    public class StatusLogDayTimed
    {
        public required DateTimeOffset Date { get; set; }
        public StatusLogDayTimedData? HeadData { get; set; } = null;

        public void AddPeriod(int periodSeconds, bool isAvailablePeriod)
        {
            if (HeadData is null)
            {
                HeadData = new StatusLogDayTimedData { PeriodSeconds = periodSeconds, IsAvailable = isAvailablePeriod };
                return;
            }

            var currentData = HeadData;
            while (currentData.NextData is not null)
            {
                currentData = currentData.NextData;
            }
            currentData.NextData = new StatusLogDayTimedData { PeriodSeconds = periodSeconds, IsAvailable = isAvailablePeriod };
        }

        public IEnumerable<StatusLogDayTimedData> GetData()
        {
            var current = HeadData;
            while (current is not null)
            {
                yield return current;
                current = current.NextData;
            }
        }


        public class StatusLogDayTimedData
        {
            public int PeriodSeconds  { get; set; } = 0;
            public bool IsAvailable { get; set; } = false;
            internal StatusLogDayTimedData? NextData { get; set; } = null;
        }
    }
}
