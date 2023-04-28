namespace Domain;

public class AnalysisResult
{
    public Message Message { get; }
    public string Topic { get; }

    public AnalysisResult(
        Message message,
        string topic)
    {
        Message = message;
        Topic = topic;
    }
}