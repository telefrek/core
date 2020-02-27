using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Represents a Json null value
    /// </summary>
    public sealed class JsonNull : JsonElement
    {
        /// <summary>
        /// Static instance for better memory management
        /// </summary>
        public static readonly JsonNull Instance = new JsonNull();

        /// <summary>
        /// Locked private constructor to force singleton
        /// </summary>
        private JsonNull() { }

        /// <inheritdoc/>
        public override JsonElementType JsonType { get => JsonElementType.Null; }

        /// <inheritdoc/>
        internal override void Print(StringBuilder builder) => builder.Append($"null");

        /// <inheritdoc/>
        internal override void PrettyPrint(StringBuilder builder, int depth) => builder.Append($"null");

        /// <inheritdoc/>
        internal override async Task WriteAsync(Stream stream, CancellationToken token)
            => await stream.WriteAsync(Encoding.UTF8.GetBytes(this.ToString()), token).ConfigureAwait(false);
    }
}
