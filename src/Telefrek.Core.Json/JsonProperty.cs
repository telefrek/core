namespace Telefrek.Core.Json
{
    /// <summary>
    /// A <see cref="Telefrek.Core.Json.JsonObject"/> property
    /// </summary>
    public sealed class JsonProperty
    {        
        /// <summary>
        /// The property name
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// The property value
        /// </summary>
        /// <value></value>
        public JsonElement Value { get; set; }
    }
}