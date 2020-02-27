namespace Telefrek.Core.Messaging
{
    /// <summary>
    /// The state of a given message
    /// </summary>
    public enum MessageState
    {
        UNKNOWN = 0,
        ABANDONED = 1,
        ABORTED = 2,
        SUCCESS = 3,
        NEW = 4,
    }
}