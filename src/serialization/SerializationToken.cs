namespace Telefrek.Serialization
{
    /// <summary>
    /// Represents the different serialization tokens recognized by the system
    /// </summary>
    public enum SerializationToken
    {
        /// <summary>
        /// Unknown token type   
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Object token type   
        /// </summary>
        Object = 1,

        /// <summary>
        /// Array token type   
        /// </summary>
        Array = 2,

        /// <summary>
        /// Null token type   
        /// </summary>
        Null = 3,

        /// <summary>
        /// Numeric token type   
        /// </summary>
        Number = 10,

        /// <summary>
        /// Long token type   
        /// </summary>
        Long = 11,

        /// <summary>
        /// Double token type   
        /// </summary>
        Double = 12,

        /// <summary>
        /// String token type   
        /// </summary>
        String = 20,
    }
}