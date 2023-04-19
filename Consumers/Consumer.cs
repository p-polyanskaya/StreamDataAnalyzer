using Application;
using Confluent.Kafka;
using Domain;
using MediatR;
using Microsoft.Extensions.Hosting;
using Serializator;

namespace Consumers;

public class Consumer : BackgroundService
{
    private readonly ConsumerBuilder<Ignore, Message> _builder;
    private readonly IMediator _mediator;

    private const string Topic = "analyses_results";
    private const string GroupId = "telegram_bot_dev";
    private const string BootstrapServers = "localhost:9092";

    public Consumer(IMediator mediator)
    {
        _mediator = mediator;
        var config = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = GroupId,
            EnableAutoCommit = false
        };

        _builder = new ConsumerBuilder<Ignore, Message>(config)
            .SetValueDeserializer(new EventDeserializer<Message>());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var consumer = _builder.Build();
        consumer.Subscribe(Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            var consumeResult = consumer.Consume(stoppingToken);

            var request = new HandleAnalyzedMessageCommand.Request(consumeResult.Message.Value);
            await _mediator.Send(request, stoppingToken);

            consumer.Commit(consumeResult);
        }

        consumer.Close();
    }
}