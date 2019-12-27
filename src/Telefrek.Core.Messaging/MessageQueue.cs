namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// A queue for processing messages
    /// </summary>
    public class MessageQueue
    {
        /// <summary>
        /// The name of the queue
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Flag for indicating if the queue is backed by disk
        /// </summary>
        /// <value></value>
        public bool Durable { get; set; }

        /// <summary>
        /// Flag for indicating if the queue should be removed after all connections are closed
        /// </summary>
        /// <value></value>
        public bool Transient { get; set; }
    }
}