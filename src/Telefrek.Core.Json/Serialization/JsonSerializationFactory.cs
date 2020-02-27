using System;
using System.Collections.Concurrent;

namespace Telefrek.Core.Json.Serialization
{
    /// <summary>
    /// Class that handles serialization registration
    /// </summary>
    public static class JsonSerializationFactory
    {
        private static readonly ConcurrentDictionary<Type, object> _serializers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Get the serializer for the given type
        /// </summary>
        /// <typeparam name="T">The type of object for serialization</typeparam>
        /// <returns>A serializer for the given type, or null</returns>
        public static IJsonSerializable<T> GetSerializer<T>() where T : class, new()
            => _serializers.TryGetValue(typeof(T), out object instance) ? (IJsonSerializable<T>)instance : null;

        /// <summary>
        /// Attempts to register the serializer
        /// </summary>
        /// <param name="serializer">The serializer to register</param>
        /// <param name="ignoreExists">Flag to indicate if this operation can override the existing implementation</param>
        /// <typeparam name="T">The type of object for serialization</typeparam>
        /// <returns>True if the operation was successsful</returns>
        public static bool TryRegister<T>(IJsonSerializable<T> serializer, bool ignoreExists = false) where T : class, new()
            => ignoreExists ? _serializers.AddOrUpdate(typeof(T), serializer, (t, o) => o ?? serializer) != null : _serializers.TryAdd(typeof(T), serializer);
    }
}