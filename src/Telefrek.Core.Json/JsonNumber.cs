using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Json representation of an number (64 bit)
    /// </summary>
    public sealed class JsonNumber : JsonPrimitive
    {
        /// <summary>
        /// The value as an int64
        /// </summary>
        /// <value></value>
        public long Value { get; set; }

        /// <summary>
        /// The value as an int32
        /// </summary>
        /// <returns></returns>
        public int ValueAsInt32 { get => (int)Value; }

        /// <inheritdoc/>
        internal override void Print(StringBuilder builder) => builder.Append($"{Value}");

        /// <inheritdoc/>
        internal override void PrettyPrint(StringBuilder builder, int depth) => builder.Append($"{Value}");

        /// <inheritdoc/>
        internal override async Task WriteAsync(Stream stream, CancellationToken token)
            => await stream.WriteAsync(Encoding.UTF8.GetBytes(this.ToString()), token).ConfigureAwait(false);
    }
}
