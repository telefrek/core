using System;
using System.Collections.Generic;

namespace Telefrek.Core.Messaging
{
    public abstract class MessageBase : IMessage
    {
        public object MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public MessageState State { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime TTL { get; set; }
        public int? DeliveryCount { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public MessageQueue Queue { get; set; }
        public string RoutingKey { get; set; }
    }
}