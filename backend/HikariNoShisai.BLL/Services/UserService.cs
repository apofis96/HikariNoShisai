using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace HikariNoShisai.BLL.Services
{
    public class UserService(HikariNoShisaiContext context, IMemoryCache memoryCache) : IUserService
    {
        private readonly HikariNoShisaiContext _context = context;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private const string CacheKeyPrefix = "user_";

        public async Task Create(long userId, long chatId, string language)
        {
            var existedUser = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (existedUser is not null)
            {
                existedUser.ChatId = chatId;
            }
            else
            {
                _context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChatId = chatId,
                    Language = language,
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<UserSettings> GetSettings(long userId)
        {
            var user = _memoryCache.Get<User>(CacheKeyPrefix + userId);

            if (user is null)
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
                if (user is null)
                    return UserSettings.None;

                _memoryCache.Set(CacheKeyPrefix + userId, user);
            }

            return (UserSettings)user.Settings;
        }

        public async Task AddSettings(long userId, UserSettings settings)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return;

            user.Settings |= (long)settings;
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + userId);
        }

        public async Task SetSettings(long userId, UserSettings settings)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return;
            user.Settings = (long)settings;
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + userId);
        }

        public async Task RemoveSettings(long userId, UserSettings settings)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return;

            user.Settings &= ~(long)settings;
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + userId);
        }

        public async Task<IEnumerable<User>> GetUsers(UserSettings settings)
        {
            long mask = (long)settings;

            var users = await _context.Users
                .Where(x => (x.Settings & mask) == mask)
                .AsNoTracking()
                .ToListAsync();

            return users;
        }

        public async Task<string> GetLanguageByUserId(long userId)
        {
            var user = _memoryCache.Get<User>(CacheKeyPrefix + userId);

            if (user is null)
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
                if (user is null)
                    return LanguageCodes.English;

                _memoryCache.Set(CacheKeyPrefix + userId, user);
            }

            return user?.Language ?? LanguageCodes.English;
        }

        public async Task SetLanguage(long userId, string language)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return;

            user.Language = language;
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + userId);
        }

        public async Task<bool> CheckUserSettings(long userId, UserSettings settings)
        {
            long mask = (long)settings;
            var user = _memoryCache.Get<User>(CacheKeyPrefix + userId);

            if (user is null)
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
                if (user is null)
                    return false;

                _memoryCache.Set(CacheKeyPrefix + userId, user);
            }

            return user is not null && (user.Settings & mask) == mask;
        }

        public async Task AddAgentShortcutToUser(long userId, AgentShortcut shortcut)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return;

            List<AgentShortcut> shortcuts;

            if (string.IsNullOrEmpty(user.AgentsShortcut))
            {
                shortcuts = [];
            }
            else
            {
                shortcuts = JsonSerializer.Deserialize<List<AgentShortcut>>(user.AgentsShortcut) ?? [];
            }

            var existingShortcut = shortcuts.FirstOrDefault(x => x.AgentId == shortcut.AgentId && x.TerminalId == shortcut.TerminalId);
            if (existingShortcut is not null)
            {
                existingShortcut.Name = shortcut.Name;
                existingShortcut.RowIndex = shortcut.RowIndex;
                existingShortcut.ColumnIndex = shortcut.ColumnIndex;
            }
            else
            {
                shortcuts.Add(shortcut);
            }

            shortcuts = shortcuts.OrderBy(x => x.AgentId).ThenBy(x => x.TerminalId).ToList();

            user.AgentsShortcut = JsonSerializer.Serialize(shortcuts);
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + userId);
        }

        public async Task<List<AgentShortcut>> GetAgentShortcuts(long userId)
        {
            var user = _memoryCache.Get<User>(CacheKeyPrefix + userId);
            if (user is null)
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
                if (user is null)
                    return [];
                _memoryCache.Set(CacheKeyPrefix + userId, user);
            }
            if (string.IsNullOrEmpty(user.AgentsShortcut))
                return [];

            return JsonSerializer.Deserialize<List<AgentShortcut>>(user.AgentsShortcut) ?? [];
        }

        public async Task RemoveAgentShortcut(long userId, int index)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null || string.IsNullOrEmpty(user.AgentsShortcut))
                return;

            var shortcuts = JsonSerializer.Deserialize<List<AgentShortcut>>(user.AgentsShortcut) ?? [];
            shortcuts.RemoveAt(index);
            shortcuts = shortcuts.OrderBy(x => x.AgentId).ThenBy(x => x.TerminalId).ToList();

            user.AgentsShortcut = JsonSerializer.Serialize(shortcuts);
            await _context.SaveChangesAsync();
            _memoryCache.Remove(CacheKeyPrefix + userId);
        }
    }
}
