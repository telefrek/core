using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// A Json object
    /// </summary>
    public sealed class JsonObject : JsonElement
    {
        /// <summary>
        /// Type is always object
        /// </summary>
        /// <value></value>
        public override JsonElementType JsonType { get => JsonElementType.Object; }

        /// <summary>
        /// A readonly copy of the current propertiesdx
        /// </summary>
        /// <value></value>
        public List<JsonProperty> Properties { get; private set; } = new List<JsonProperty>();

        /// <summary>
        /// Checks to see if the object has the given property
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the property exists</returns>
        public bool Has(string propertyName, bool ignoreCase = true)
            => Properties.Any(p => string.Equals(p.Name, propertyName, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));

        /// <inheritdoc/>
        internal override void Print(StringBuilder builder)
        {
            if (Properties.Count > 0)
            {
                builder.Append("{");
                var l = Properties.Count - 1;
                for (var i = 0; i < l; ++i)
                {
                    builder.Append($"\"{Properties[i].Name}\":");
                    Properties[i].Value.Print(builder);
                    builder.Append(",");
                }
                builder.Append($"\"{Properties[l].Name}\":");
                Properties[l].Value.Print(builder);
                builder.Append("}");
            }
        }


        /// <inheritdoc/>
        internal override void PrettyPrint(StringBuilder builder, int depth)
        {
            if (Properties.Count > 0)
            {
                builder.Append("{\n");
                var l = Properties.Count - 1;
                ++depth;
                for (var i = 0; i < l; ++i)
                {
                    builder.Append($"{new string('\t', depth)}\"{Properties[i].Name}\" : ");
                    Properties[i].Value.PrettyPrint(builder, depth);
                    builder.Append(",\n");
                }
                builder.Append($"{new string('\t', depth)}\"{Properties[l].Name}\" : ");
                Properties[l].Value.PrettyPrint(builder, depth);
                builder.Append($"\n{new string('\t', --depth)}}}");
            }
        }

        /// <inheritdoc/>
        internal override async Task WriteAsync(Stream stream, CancellationToken token)
            => await stream.WriteAsync(Encoding.UTF8.GetBytes(this.ToString()), token).ConfigureAwait(false);
    }
}
