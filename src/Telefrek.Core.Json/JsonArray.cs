using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Represents an array of JsonElements
    /// </summary>
    public sealed class JsonArray : JsonElement
    {
        /// <inheritdoc/>
        public override JsonElementType JsonType { get => JsonElementType.Array; }

        /// <summary>
        /// The collection of items
        /// </summary>
        /// <value></value>
        public List<JsonElement> Items { get; private set; } = new List<JsonElement>();

        /// <inheritdoc/>
        internal override void Print(StringBuilder builder)
        {
            if (Items.Count > 0)
            {
                builder.Append("[");
                var l = Items.Count - 1;
                for (var i = 0; i < l; ++i)
                {
                    Items[i].Print(builder);
                    builder.Append(",");
                }
                Items[l].Print(builder);
                builder.Append("]");
            }
        }

        /// <inheritdoc/>
        internal override void PrettyPrint(StringBuilder builder, int depth)
        {
            builder.Append($"[\n{new string('\t', depth)}");
            if (Items.Count > 0)
            {
                var l = Items.Count - 1;
                for (var i = 0; i < l; ++i)
                {
                    Items[i].PrettyPrint(builder, depth + 1);
                    builder.Append($",\n{new string('\t', depth)}");
                }
                Items[l].PrettyPrint(builder, depth + 1);
            }
            builder.Append($"\n{new string('\t', depth)}]");
        }

        /// <inheritdoc/>
        internal override async Task WriteAsync(Stream stream, CancellationToken token)
            => await stream.WriteAsync(Encoding.UTF8.GetBytes(this.ToString()), token).ConfigureAwait(false);
    }
}
