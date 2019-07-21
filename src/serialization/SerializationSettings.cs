namespace Telefrek.Serialization
{
    /// <summary>
    /// Class to control how to serialize objects
    /// </summary>
    public class SerializationSettings
    {
        /// <summary>
        /// Flag to include null values in serialization
        /// </summary>
        /// <value>Default as null</value>
        public bool IncludeNull { get; set; }

        /// <summary>
        /// Flag to treat default values the same as null
        /// </summary>
        /// <value>Default as true</value>
        public bool DefaultAsNull { get; set; } = true;
    }
}