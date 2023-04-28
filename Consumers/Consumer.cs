using Application;
using Confluent.Kafka;
using Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Spark.Sql;
using Microsoft.Spark.Sql.Streaming;
using Options;
using Serializator;

namespace Consumers;

public class Consumer : BackgroundService
{
    private readonly IOptions<ConsumersSettings> _consumerSettings;
    private readonly ConsumerBuilder<Ignore, Message> _builder;
    private readonly IServiceProvider _serviceProvider;

    public Consumer(IOptions<ConsumersSettings> consumerSettings, IServiceProvider serviceProvider)
    {
        _consumerSettings = consumerSettings;
        _serviceProvider = serviceProvider;
        var config = new ConsumerConfig
        {
            BootstrapServers = _consumerSettings.Value.ConsumerForSendingMessagesToAnalyze.BootstrapServers,
            GroupId = _consumerSettings.Value.ConsumerForSendingMessagesToAnalyze.GroupId,
            EnableAutoCommit = false
        };

        _builder = new ConsumerBuilder<Ignore, Message>(config)
            .SetValueDeserializer(new EventDeserializer<Message>());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var consumer = _builder.Build();
        consumer.Subscribe(_consumerSettings.Value.ConsumerForSendingMessagesToAnalyze.Topic);
        while (!stoppingToken.IsCancellationRequested)
        {
            /*
            var spark = SparkSession
                .Builder()
                .AppName("Streaming example with a UDF")
                .GetOrCreate();

            var dataFrame = spark
                .ReadStream()
                .Format("kafka")
                .Option("host", "kafka.bootstrap.servers")
                .Option("port", _consumersOptions.Value.ConsumerForSendingMessagesToAnalyze.BootstrapServers)
                .Option("subscribe",  _consumersOptions.Value.ConsumerForSendingMessagesToAnalyze.Topic)
                .Load();
            dataFrame
                .WriteStream()
                .Trigger(Trigger.ProcessingTime(5000))
                .Start();
            dataFrame.Show();
            */
            
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                Console.WriteLine(consumeResult.Message.Value.Text);
                var analysisResult = new AnalysisResult(consumeResult.Message.Value, "common");
                var request = new HandleAnalyzedMessageCommand.Request(new[] { analysisResult });

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(request, stoppingToken);
                consumer.Commit(consumeResult);
            }
            catch(Exception ex)
            {
            }
        }
    }
}