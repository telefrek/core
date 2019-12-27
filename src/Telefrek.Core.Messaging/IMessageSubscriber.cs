using System;
using System.Threading.Tasks;

namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// Allows subscribing to a quuee and handle message processing
    /// </summary>
    public interface IMessageSubscriber : IDisposable
    {
        /// <summary>
        /// Adds an asynchronous listener to handle messages
        /// </summary>
        /// <param name="listener">The listener to route messages to</param>
        void AddListener(AsyncMessagingListener listener);

        /// <summary>
        /// The binding information for the queue and filtering
        /// </summary>
        /// <value></value>
        MessageQueueBinding Binding { get; }
    }
}