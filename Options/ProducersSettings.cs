namespace Options;

public class ProducersSettings
{
    public ProducerSettings ProducerForSendingMessagesToAnalyze { get; set; }
    public ProducerSettings ProducerForAnalyzedMessages { get; set; }
}