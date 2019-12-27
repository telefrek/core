using System;
using System.Runtime.Serialization;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Signifies there was an issue processing the source as Json
    /// </summary>
    [System.Serializable]
    public class InvalidJsonFormatException : Exception
    {
        public InvalidJsonFormatException() { }
        public InvalidJsonFormatException(string message) : base(message) { }
        public InvalidJsonFormatException(string message, Exception inner) : base(message, inner) { }
        protected InvalidJsonFormatException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}