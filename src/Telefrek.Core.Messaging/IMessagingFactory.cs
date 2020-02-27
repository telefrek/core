using System;
using System.Threading.Tasks;

namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// Factory for creating messaging resources
    /// </summary>
    public interface IMessagingFactory : IDisposable
    {
        /// <summary>
        /// Asynchronously create a publisher
        /// </summary>
        /// <param name="queue">The queue to create a publisher for</param>
        /// <returns>A <see cref="Telefrek.Core.Messaging.IMessagePublisher"/> that can be used to publish messages</returns>
        Task<IMessagePublisher> CreatePublisherAsync(MessageQueue queue);

        /// <summary>
        /// Asynchronously create a subscriber
        /// </summary>
        /// <param name="binding">The queue binding information for creating a subscriber.</param>
        /// <returns>A <see cref="Telefrek.Core.Messaging.IMessageSubscriber"/> that can be used to subscribe to messages</returns>
        Task<IMessageSubscriber> CreateSubscriberAsync(MessageQueueBinding binding);

        /// <summary>
        /// Create a new message of the given type
        /// </summary>
        /// <typeparam name="T">The type of message to create</typeparam>
        /// <returns>A message of the specified type</returns>
        T CreateMessage<T>() where T : class, IMessage;
    }
}