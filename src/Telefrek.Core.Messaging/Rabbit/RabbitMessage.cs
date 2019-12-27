using Telefrek.Core.Json;

namespace Telefrek.Core.Messaging.Rabbit
{
    public class RabbitMessage : MessageBase, IJsonMessage, ITextMessage
    {
        /// <summary>
        /// Internal constructor
        /// </summary>
        internal RabbitMessage() { }

        /// <inheritdoc/>
        public JsonElement JsonValue { get; set; } = JsonNull.Instance;

        /// <inheritdoc/>
        public string StringValue { get; set; }
    }
}