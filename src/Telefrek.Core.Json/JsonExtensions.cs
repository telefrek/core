using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using System;
using Telefrek.Core.Json.Serialization;
using System.Threading;

namespace Telefrek.Core.Json
{
    public static partial class TelefrekExtensions
    {
        /// <summary>
        /// Converts the element to it's string representation
        /// </summary>
        /// <param name="element">The element to convert</param>
        /// <param name="pretty">Flag to indicate if the printing should be formatted with whitespace</param>
        /// <returns>A formatted string for the element</returns>
        public static string ToJson(this JsonElement element, bool pretty = false)
        {
            var sb = new StringBuilder();
            if (pretty) element.PrettyPrint(sb, 0);
            else element.Print(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Converts the instance into it's Json string representation
        /// </summary>
        /// <param name="instance">The instance to serialize</param>
        /// <param name="pretty">Flag to indicate if the printing should be formatted with whitespace</param>
        /// <typeparam name="T">The type of object being serialized</typeparam>
        /// <exception cref="Telefrek.Core.Json.Serialization.JsonSerializationException">If the object has no serializer</exception>
        public static string ToJson<T>(this T instance, bool pretty = false) where T : class, new()
            => instance.AsJson().ToJson(pretty);

        /// <summary>
        /// Parses the string into a JsonElement
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns>The JsonElement representing the string</returns>
        /// <exception cref="Telefrek.Core.Json.Serialization.JsonSerializationException">If the object has no serializer</exception>
        public static JsonElement AsJson<T>(this T instance) where T : class, new()
        {
            var serializer = JsonSerializationFactory.GetSerializer<T>();
            if (serializer == null) throw new JsonSerializationException("No serializer available for type: " + typeof(T).Name);
            return serializer.Serialize(instance);
        }

        /// <summary>
        /// Parses the string into a JsonElement
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns>The JsonElement representing the string</returns>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the string is not proper Json</exception>
        public static JsonElement AsJson(this string s) => JsonParser.ParseBuffer(new ReadOnlySequence<byte>(new Memory<byte>(Encoding.UTF8.GetBytes(s ?? ""))));

        /// <summary>
        /// Parses the bytes into a JsonElement
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>A JsonElement representing the bytes</returns>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the buffer is not proper Json</exception>
        public static JsonElement AsJson(this byte[] bytes) => JsonParser.ParseBuffer(new ReadOnlySequence<byte>(new Memory<byte>(bytes ?? new byte[0])));

        /// <summary>
        /// Parses the stream into a JsonElement asynchronously
        /// </summary>
        /// <param name="stream">The stream to parse</param>
        /// <param name="token">The cancellation token for parsing</param>
        /// <param name="leaveOpen">Flag to indicate if the stream should be closed after parsing</param>
        /// <returns>A JsonElement representing the stream</returns>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the stream is not proper Json</exception>
        public static async Task<JsonElement> AsJsonAsync(this Stream stream, CancellationToken token = default(CancellationToken), bool leaveOpen = true)
            => await JsonParser.ParseAsync(stream, token, leaveOpen).ConfigureAwait(false);

        /// <summary>
        /// Parse an object from it's Json string representation
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <typeparam name="T">The type object to deserialize into</typeparam>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the stream is not proper Json</exception>
        /// <exception cref="Telefrek.Core.Json.Serialization.JsonSerializationException">If the object has no serializer</exception>
        public static T FromJson<T>(this string s) where T : class, new()
        {
            var serializer = JsonSerializationFactory.GetSerializer<T>();
            if (serializer == null) throw new JsonSerializationException("No serializer available for type: " + typeof(T).Name);
            return serializer.Deserialize(JsonParser.ParseBuffer(new ReadOnlySequence<byte>(new Memory<byte>(Encoding.UTF8.GetBytes(s ?? "")))));
        }

        /// <summary>
        /// Parse an object from it's Json byte representation
        /// </summary>
        /// <param name="bytes"></param>
        /// <typeparam name="T">The type object to deserialize into</typeparam>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the stream is not proper Json</exception>
        /// <exception cref="Telefrek.Core.Json.Serialization.JsonSerializationException">If the object has no serializer</exception>
        public static T FromJson<T>(this byte[] bytes) where T : class, new()
        {
            var serializer = JsonSerializationFactory.GetSerializer<T>();
            if (serializer == null) throw new JsonSerializationException("No serializer available for type: " + typeof(T).Name);
            return serializer.Deserialize(JsonParser.ParseBuffer(new ReadOnlySequence<byte>(new Memory<byte>(bytes ?? new byte[0]))));
        }

        /// <summary>
        /// Parses an object from it's Json stream representation asynchronously
        /// </summary>
        /// <param name="stream">The stream to parse</param>
        /// <param name="token">The cancellation token for parsing</param>
        /// <param name="leaveOpen">Flag to indicate if the stream should be closed after parsing</param>
        /// <returns>A JsonElement representing the stream</returns>
        /// <exception cref="Telefrek.Core.Json.InvalidJsonFormatException">If the stream is not proper Json</exception>
        /// <exception cref="Telefrek.Core.Json.Serialization.JsonSerializationException">If the object has no serializer</exception>
        public static async Task<T> FromJsonAsync<T>(this Stream stream, CancellationToken token = default(CancellationToken), bool leaveOpen = true) where T : class, new()
        {
            var serializer = JsonSerializationFactory.GetSerializer<T>();
            if (serializer == null) throw new JsonSerializationException("No serializer available for type: " + typeof(T).Name);
            return serializer.Deserialize(await JsonParser.ParseAsync(stream, token, leaveOpen).ConfigureAwait(false));
        }

        #region Helper Methods
        /// <summary>
        /// Flag to indicate if the JsonElement is an object
        /// </summary>
        /// <param name="element">The element to inspect</param>
        /// <returns>True if the element is an object</returns>
        public static bool IsJsonObject(this JsonElement element) => element.JsonType == JsonElementType.Object;

        /// <summary>
        /// Flag to indicate if the JsonElement is an array
        /// </summary>
        /// <param name="element">The element to inspect</param>
        /// <returns>True if the element is an array</returns>
        public static bool IsJsonArray(this JsonElement element) => element.JsonType == JsonElementType.Array;

        /// <summary>
        /// Flag to indicate if the JsonElement is a primitive
        /// </summary>
        /// <param name="element">The element to inspect</param>
        /// <returns>True if the element is a primitive</returns>
        public static bool IsJsonPrimitive(this JsonElement element) => element.JsonType == JsonElementType.Primitive;

        /// <summary>
        /// Flag to indicate if the JsonElement is a JsonNull
        /// </summary>
        /// <param name="element">The element to inspect</param>
        /// <returns>True if the element is a JsonNull</returns>
        public static bool IsJsonNull(this JsonElement element) => element.JsonType == JsonElementType.Null;

        /// <summary>
        /// Helper method to cast the JsonElement as a JsonArray
        /// </summary>
        /// <param name="element">The element to modify</param>
        /// <returns>A JsonArray if the object is valid</returns>
        /// <exception cref="Telefrek.Core.Json.JsonCastException">If the element is not an array</exception>
        public static JsonArray AsJsonArray(this JsonElement element) => element.IsJsonArray() ? (element as JsonArray) : throw new JsonCastException();

        /// <summary>
        /// Helper method to cast the JsonElement as a JsonObject
        /// </summary>
        /// <param name="element">The element to modify</param>
        /// <returns>A JsonObject if the object is valid</returns>
        /// <exception cref="Telefrek.Core.Json.JsonCastException">If the element is not an object</exception>
        public static JsonObject AsJsonObject(this JsonElement element) => element.IsJsonObject() ? (element as JsonObject) : throw new JsonCastException();

        /// <summary>
        /// Helper method to cast the JsonElement to it's string value
        /// </summary>
        /// <param name="element">The element to modify</param>
        /// <returns>The string value of the object</returns>
        /// /// <exception cref="System.InvalidCastException">If the element is not a string</exception>
        public static string AsString(this JsonElement element) => (element as JsonString).Value;

        /// <summary>
        /// Helper method to cast the JsonElement to it's double value
        /// </summary>
        /// <param name="element">The element to modify</param>
        /// <returns>The string value of the object</returns>
        /// /// <exception cref="System.InvalidCastException">If the element is not a double</exception>
        public static double AsDouble(this JsonElement element) => (element as JsonDouble).Value;

        /// <summary>
        /// Helper method to cast the JsonElement to it's long value
        /// </summary>
        /// <param name="element">The element to modify</param>
        /// <returns>The string value of the object</returns>
        /// /// <exception cref="System.InvalidCastException">If the element is not a long</exception>
        public static long AsLong(this JsonElement element) => (element as JsonNumber).Value;

        /// <summary>
        /// Helper method to cast the JsonElement to it's integer value
        /// </summary>
        /// <param name="element">The element to modify</param>
        /// <returns>The string value of the object</returns>
        /// /// <exception cref="System.InvalidCastException">If the element is not an integer</exception>
        public static int AsInteger(this JsonElement element) => (element as JsonNumber).ValueAsInt32;

        /// <summary>
        /// Helper method to cast the JsonElement to it's boolean value
        /// </summary>
        /// <param name="element">The element to modify</param>
        /// <returns>The string value of the object</returns>
        /// /// <exception cref="System.InvalidCastException">If the element is not a boolean</exception>
        public static bool AsBool(this JsonElement element) => (element as JsonBool).Value;
        #endregion

        #region JsonObject manipulations
        /// <summary>
        /// Adds a property with the given name and value
        /// </summary>
        /// <param name="jsonObject">The object to manipulate</param>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        /// <typeparam name="T">The value type</typeparam>
        public static void Add<T>(this JsonObject jsonObject, string name, T value) where T : JsonElement
        => jsonObject.Properties.Add(new JsonProperty { Name = name, Value = value });

        /// <summary>
        /// Adds a property with the given name and value
        /// </summary>
        /// <param name="jsonObject">The object to manipulate</param>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public static void Add(this JsonObject jsonObject, string name, bool value)
        => jsonObject.Properties.Add(new JsonProperty { Name = name, Value = value });

        /// <summary>
        /// Adds a property with the given name and value
        /// </summary>
        /// <param name="jsonObject">The object to manipulate</param>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public static void Add(this JsonObject jsonObject, string name, string value)
        => jsonObject.Properties.Add(new JsonProperty { Name = name, Value = (JsonElement)value });

        /// <summary>
        /// Adds a property with the given name and value
        /// </summary>
        /// <param name="jsonObject">The object to manipulate</param>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public static void Add(this JsonObject jsonObject, string name, int value)
        => jsonObject.Properties.Add(new JsonProperty { Name = name, Value = (JsonElement)value });

        /// <summary>
        /// Adds a property with the given name and value
        /// </summary>
        /// <param name="jsonObject">The object to manipulate</param>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        public static void Add(this JsonObject jsonObject, string name, double value)
        => jsonObject.Properties.Add(new JsonProperty { Name = name, Value = (JsonElement)value });
        #endregion

        /// <summary>
        /// Verifies if the byte is a numeric character
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsNumeric(this byte b) => ((b >= (byte)'0') && (b <= (byte)'9')) || b == (byte)'.' || b == (byte)'-';
    }
}