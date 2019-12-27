using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Represents a floating point object
    /// </summary>
    public sealed class JsonDouble : JsonPrimitive
    {
        /// <summary>
        /// The double value for this object
        /// </summary>
        /// <value></value>
        public double Value { get; set; }

        /// <inheritdoc/>
        internal override void Print(StringBuilder builder) => builder.Append($"{Value}");

        /// <inheritdoc/>
        internal override void PrettyPrint(StringBuilder builder, int depth) => builder.Append($"{Value}");

        /// <inheritdoc/>
        internal override async Task WriteAsync(Stream stream, CancellationToken token)
            => await stream.WriteAsync(Encoding.UTF8.GetBytes(this.ToString()), token).ConfigureAwait(false);
    }
}
