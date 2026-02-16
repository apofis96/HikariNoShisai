using HikariNoShisai.Common.Models;

namespace HikariNoShisai.Common.Interfaces
{
    public interface IMessageQueue
    {
        void Send<T>(string topic, T message);
        public Message<T>? Recive<T>(string topic);
        void Clear(string topic);
        void ClearAll();
    }
}
