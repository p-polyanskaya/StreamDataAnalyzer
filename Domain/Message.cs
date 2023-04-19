namespace Domain;

public class Message
{
    public Guid Id { get; }
    public string UserName { get; }
    public DateTime TimeOfMessage { get; }
    public byte[] MessageText { get; }
    public string Source { get; }

    public Message(
        Guid id,
        string userName,
        byte[] messageText, 
        DateTime timeOfMessage, 
        string source)
    {
        Id = id;
        UserName = userName;
        MessageText = messageText;
        TimeOfMessage = timeOfMessage;
        Source = source;
    }
}