namespace Telefrek.Core.Json.Serialization
{
    /// <summary>
    /// Interface to define serialization interaction for Json objects
    /// </summary>
    /// <typeparam name="T">The type of object this serializer handles</typeparam>
    public interface IJsonSerializable<T> where T : class, new()
    {
        /// <summary>
        /// Deserialize the JsonElement as a value
        /// </summary>
        /// <param name="element">The element to deserialize</param>
        /// <returns>The equivalent instance</returns>
        T Deserialize(JsonElement element);

        /// <summary>
        /// Serialize the instance into a JsonElement
        /// </summary>
        /// <param name="instance">The instance to serialize</param>
        /// <returns>An equivalent JsonElement</returns>
        JsonElement Serialize(T instance);
    }
}