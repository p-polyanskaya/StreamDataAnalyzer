namespace Domain;

public class Message
{
    public Guid Id { get; }
    public string Author { get; }
    public DateTime TimeOfMessage { get; }
    public string Text { get; }
    public string Source { get; }

    public Message(
        Guid id,
        string author,
        string text,
        DateTime timeOfMessage,
        string source)
    {
        Id = id;
        Author = author;
        Text = text;
        TimeOfMessage = timeOfMessage;
        Source = source;
    }
}