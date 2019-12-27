using System;
using System.Threading.Tasks;

namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// Asynchronous listener for message processing
    /// </summary>
    public abstract class AsyncMessagingListener
    {
        /// <summary>
        /// Processes the message asynchronously
        /// </summary>
        /// <param name="message">The message to proces.s</param>
        /// <typeparam name="T">The type for this message.</typeparam>
        /// <returns>A Task for tracking completion.</returns>
        public abstract Task HandleMessageAsync<T>(T message) where T : IMessage;
    }
}