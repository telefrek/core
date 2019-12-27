using System;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Abstract Json primitive class
    /// </summary>
    public abstract class JsonPrimitive : JsonElement
    {
        /// <inheritdoc/>
        public override JsonElementType JsonType { get => JsonElementType.Primitive; }
    }
}
