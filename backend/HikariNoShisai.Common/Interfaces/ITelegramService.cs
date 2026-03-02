namespace HikariNoShisai.Common.Interfaces
{
    public interface ITelegramService
    {
        Task<string> Handle(long userId, string message);
    }
}
