using System;
using System.Runtime.Serialization;

namespace Telefrek.Core.Json.Serialization
{
    /// <summary>
    /// Exceptions resulting from serialization of Json objects
    /// </summary>
    [Serializable]
    public class JsonSerializationException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public JsonSerializationException() { }

        /// <summary>
        /// Message constructor
        /// </summary>
        /// <param name="message">The reason for the exception</param>
        public JsonSerializationException(string message) : base(message) { }

        /// <summary>
        /// Message and cause constructor
        /// </summary>
        /// <param name="message">The reason for the exception</param>
        /// <param name="inner">The root cause</param>
        public JsonSerializationException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">The current information</param>
        /// <param name="context">The current streaming context</param>
        protected JsonSerializationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}