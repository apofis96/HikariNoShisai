namespace HikariNoShisai.Common.Interfaces
{
    public interface IUserService
    {
        Task Create(long userId, long chatId);
    }
}
