namespace HikariNoShisai.Common.Interfaces
{
    public interface ITelegramService
    {
        Task<string> Handle(string message);
    }
}
