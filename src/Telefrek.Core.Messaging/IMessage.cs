using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// The base message class implementation
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The unique message id
        /// </summary>
        /// <value></value>
        object MessageId { get; set; }

        /// <summary>
        /// The correlation id for the message
        /// </summary>
        /// <value></value>
        Guid CorrelationId { get; set; }

        /// <summary>
        /// The current state of the message
        /// </summary>
        /// <value></value>
        MessageState State { get; set; }

        /// <summary>
        /// The time the message was created
        /// </summary>
        /// <value></value>
        DateTime Created { get; set; }

        /// <summary>
        /// The time when the message expires
        /// </summary>
        /// <value></value>
        DateTime TTL { get; set; }

        /// <summary>
        /// The number of times this message was delivered
        /// </summary>
        /// <value></value>
        int? DeliveryCount { get; set; }

        /// <summary>
        /// Message headers
        /// </summary>
        /// <value></value>
        Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Message properties
        /// </summary>
        /// <value></value>
        Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// The queue this message came from
        /// </summary>
        /// <value></value>
        MessageQueue Queue { get; set; }

        /// <summary>
        /// The message routing key
        /// </summary>
        /// <value></value>
        string RoutingKey { get; set; }
    }
}