using HikariNoShisai.Common.Constants;

namespace HikariNoShisai.Common.Interfaces
{
    public interface IUserService
    {
        Task Create(long userId, long chatId, string language);
        Task<UserSettings> GetSettings(long userId);
        Task AddSettings(long userId, UserSettings settings);
        Task RemoveSettings(long userId, UserSettings settings);
        Task<IEnumerable<long>> GetChatIds(UserSettings settings);
    }
}
