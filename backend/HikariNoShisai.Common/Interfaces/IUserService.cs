using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Models;

namespace HikariNoShisai.Common.Interfaces
{
    public interface IUserService
    {
        Task Create(long userId, long chatId, string language);
        Task<UserSettings> GetSettings(long userId);
        Task AddSettings(long userId, UserSettings settings);
        Task SetSettings(long userId, UserSettings settings);
        Task SetLanguage(long userId, string language);
        Task RemoveSettings(long userId, UserSettings settings);
        Task<IEnumerable<User>> GetUsers(UserSettings settings);
        Task<string> GetLanguageByUserId(long userId);
        Task<bool> CheckUserSettings(long userId, UserSettings settings);
        Task AddAgentShortcutToUser(long userId, AgentShortcut shortcut);
        Task<List<AgentShortcut>> GetAgentShortcuts(long userId);
        Task RemoveAgentShortcuts(long userId, Guid AgentTerminalId);
    }
}
