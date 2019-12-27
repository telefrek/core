using System;
using System.Threading.Tasks;

namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// Allows messages to be published to a queue
    /// </summary>
    public interface IMessagePublisher : IDisposable
    {
        /// <summary>
        /// Attempts to publish the message asynchronously
        /// </summary>
        /// <param name="message">The message to publish</param>
        /// <typeparam name="T">The type of message</typeparam>
        /// <returns>True if the operation succeeded</returns>
        Task<bool> TryPublishAsync<T>(T message) where T : IMessage;

        /// <summary>
        /// The destination queue to publish to
        /// </summary>
        /// <value></value>
        MessageQueue Queue { get; }
    }
}