namespace HikariNoShisai.Common.Interfaces
{
    public interface ISettingsService
    {
        Task<TimeSpan> GetTimezoneOffset();
        Task<int> GetTimezoneMinutes();
        Task SetTimezoneOffset(int offset);
    }
}
