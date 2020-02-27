namespace Telefrek.Core.Messaging
{
    public interface ITextMessage : IMessage
    {
        string StringValue { get; set; }
    }
}