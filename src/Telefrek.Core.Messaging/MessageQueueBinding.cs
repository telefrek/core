namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// Binding instructions for connecting to the queue
    /// </summary>
    public class MessageQueueBinding
    {
        /// <summary>
        /// The <see cref="Telefrek.Core.Messaging.MessageQueue"/> to binding information
        /// </summary>
        /// <value></value>
        public MessageQueue Queue { get; set; }

        /// <summary>
        /// The filter to apply to messages for acceptance
        /// </summary>
        /// <value></value>
        public string MessageFilter { get; set; }

        /// <summary>
        /// The id for this binding
        /// </summary>
        /// <value></value>
        public string BindingId { get; set; }

        /// <summary>
        /// If this should be a single consumer binding
        /// </summary>
        /// <value></value>
        public bool Exclusive { get; set; }
    }
}