using HikariNoShisai.Common.Constants;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.BLL.Services
{
    public class UserService(HikariNoShisaiContext context) : IUserService
    {
        private readonly HikariNoShisaiContext _context = context;

        public async Task Create(long userId, long chatId)
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
                    ChatId = chatId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<UserSettings> GetSettings(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return UserSettings.None;

            return (UserSettings)user.Settings;
        }

        public async Task AddSettings(long userId, UserSettings settings)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return;

            user.Settings |= (long)settings;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSettings(long userId, UserSettings settings)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user is null)
                return;

            user.Settings &= ~(long)settings;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<long>> GetChatIds(UserSettings settings)
        {
            long mask = (long)settings;

            var chatIds = await _context.Users
                .Where(x => (x.Settings & mask) == mask)
                .Select(x => x.ChatId)
                .ToListAsync();

            return chatIds;
        }
    }
}
