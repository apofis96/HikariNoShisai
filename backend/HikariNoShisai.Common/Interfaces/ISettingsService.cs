namespace HikariNoShisai.Common.Interfaces
{
    public interface ISettingsService
    {
        Task<TimeSpan> GetTimezoneOffset();
        Task SetTimezoneOffset(int offset);
    }
}
