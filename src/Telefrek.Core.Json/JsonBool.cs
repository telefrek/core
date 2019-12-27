using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// A Json bool object
    /// </summary>
    public sealed class JsonBool : JsonPrimitive
    {
        /// <summary>
        /// The boolean value for this element
        /// </summary>
        /// <value></value>
        public bool Value { get; set; }

        /// <inheritdoc/>
        internal override void Print(StringBuilder builder) => builder.Append(Value ? "true" : "false");

        /// <inheritdoc/>
        internal override void PrettyPrint(StringBuilder builder, int depth) => builder.Append(Value ? "true" : "false");

        /// <inheritdoc/>
        internal override async Task WriteAsync(Stream stream, CancellationToken token)
            => await stream.WriteAsync(Encoding.UTF8.GetBytes(this.ToString()), token).ConfigureAwait(false);
    }
}
