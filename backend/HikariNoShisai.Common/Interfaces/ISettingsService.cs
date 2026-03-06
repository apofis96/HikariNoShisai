namespace HikariNoShisai.Common.Interfaces
{
    public interface ISettingsService
    {
        Task<int> GetTimezoneOffset();
        Task SetTimezoneOffset(int offset);
    }
}
