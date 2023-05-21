namespace Domain;

public class FailedMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; }
    public AnalysisResult AnalysisResult { get; set; }
}