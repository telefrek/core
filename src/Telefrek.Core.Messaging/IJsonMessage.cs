using Telefrek.Core.Json;

namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// A message that uses Json for the payload
    /// </summary>
    public interface IJsonMessage : IMessage
    {
        JsonElement JsonValue { get; set; }
    }
}