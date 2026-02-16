using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.Common.Models;
using System.Collections.Concurrent;

namespace HikariNoShisai.BLL.Infrastructure
{
    public class MessageQueue : IMessageQueue
    {
        private static readonly ConcurrentDictionary<string, ConcurrentQueue<object>> _queues = new();

        public void Send<T>(string topic, T message)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

            var queue = _queues.GetOrAdd(topic, _ => new ConcurrentQueue<object>());

            var wrappedMessage = new Message<T>
            {
                Data = message,
                Timestamp = DateTime.UtcNow,
                Topic = topic
            };

            queue.Enqueue(wrappedMessage);
        }

        public Message<T>? Recive<T>(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

            if (_queues.TryGetValue(topic, out var queue))
                if (queue.TryDequeue(out object? item))
                    return item as Message<T>;

            return null;
        }

        public void Clear(string topic)
        {
            _queues.TryRemove(topic, out _);
        }

        public void ClearAll()
        {
            _queues.Clear();
        }
    }
}
