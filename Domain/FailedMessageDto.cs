namespace Domain;

public class FailedMessageDto
{
    public Guid Id { get; set; }
    public string Topic { get; set; }
    public string AnalysisResult { get; set; }
}