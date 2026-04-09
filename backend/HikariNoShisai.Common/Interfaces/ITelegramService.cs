using HikariNoShisai.Common.Models;

namespace HikariNoShisai.Common.Interfaces
{
    public interface ITelegramService
    {
        Task<TelegramHtmlMessage> Handle(long userId, string message);
    }
}
