using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Base class for all Json elements
    /// </summary>
    public abstract class JsonElement
    {
        /// <summary>
        /// Get the element type
        /// </summary>
        /// <value></value>
        public abstract JsonElementType JsonType { get; }

        /// <summary>
        /// Write this element to the builder
        /// </summary>
        /// <param name="builder">The builder used at the root element</param>
        internal abstract void Print(StringBuilder builder);

        /// <summary>
        /// Write this element to the builder
        /// </summary>
        /// <param name="builder">The builder used at the root element</param>
        /// <param name="depth">The current depth</param>
        internal abstract void PrettyPrint(StringBuilder builder, int depth);

        /// <summary>
        /// Asynchronously write this element to the stream
        /// </summary>
        /// <param name="stream">The target stream</param>
        /// <param name="token">A token for cancelling the task</param>
        /// <returns>A task for tracking completion</returns>
        internal abstract Task WriteAsync(Stream stream, CancellationToken token);

        /// <summary>
        /// Implicit cast to a JsonElement from a primitive value
        /// </summary>
        /// <param name="d">The double to wrap</param>
        public static implicit operator JsonElement(double d) => new JsonDouble { Value = d };

        /// <summary>
        /// Implicit cast to a JsonElement from a primitive value
        /// </summary>
        /// <param name="b">The boolean to wrap</param>
        public static implicit operator JsonElement(bool b) => new JsonBool { Value = b };

        /// <summary>
        /// Implicit cast to a JsonElement from a primitive value
        /// </summary>
        /// <param name="i">The integer to wrap</param>
        public static implicit operator JsonElement(int i) => new JsonNumber { Value = i };

        /// <summary>
        /// Implicit cast to a JsonElement from a primitive value
        /// </summary>
        /// <param name="l">The long to wrap</param>
        public static implicit operator JsonElement(long l) => new JsonNumber { Value = l };

        /// <summary>
        /// Implicit cast to a JsonElement from a primitive value
        /// </summary>
        /// <param name="s">The string to wrap</param>
        public static implicit operator JsonElement(string s) => string.IsNullOrEmpty(s) ? (JsonElement)JsonNull.Instance : new JsonString { Value = s };

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            this.Print(sb);
            return sb.ToString();
        }
    }
}
