namespace Options;

public class ConsumerSettings
{
    public string Topic { get; set; }
    public string GroupId { get; set; }
    public string BootstrapServers { get; set; }
}