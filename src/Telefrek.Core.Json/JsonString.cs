using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Represents a string primitive
    /// </summary>
    public sealed class JsonString : JsonPrimitive
    {
        /// <summary>
        /// The wrapped value
        /// </summary>
        /// <value></value>
        public string Value { get; set; }

        /// <inheritdoc/>
        internal override void Print(StringBuilder builder) => builder.Append($"\"{Value}\"");

        /// <inheritdoc/>
        internal override void PrettyPrint(StringBuilder builder, int depth) => builder.Append($"\"{Value}\"");

        /// <inheritdoc/>
        internal override async Task WriteAsync(Stream stream, CancellationToken token)
            => await stream.WriteAsync(Encoding.UTF8.GetBytes(this.ToString()), token).ConfigureAwait(false);
    }
}
