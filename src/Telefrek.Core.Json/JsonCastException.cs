using System;
using System.Runtime.Serialization;

namespace Telefrek.Core.Json
{
    /// <summary>
    /// Signifies the attempted cast was invalid
    /// </summary>
    [System.Serializable]
    public class JsonCastException : Exception
    {
        public JsonCastException() { }
        public JsonCastException(string message) : base(message) { }
        public JsonCastException(string message, Exception inner) : base(message, inner) { }
        protected JsonCastException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}