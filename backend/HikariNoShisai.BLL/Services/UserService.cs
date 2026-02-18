using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.BLL.Services
{
    public class UserService(HikariNoShisaiContext context): IUserService
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
    }
}
