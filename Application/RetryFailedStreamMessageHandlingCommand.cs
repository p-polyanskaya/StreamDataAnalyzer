using Confluent.Kafka;
using Domain;
using MediatR;
using Microsoft.Extensions.Options;
using Options;
using Redis;
using Serializator;

namespace Application;

public static class RetryFailedStreamMessageHandlingCommand
{
    public record Request() : IRequest<Unit>;

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly RedisOperations _redis;
        private readonly IOptions<ProducersSettings> _producersOptions;

        public Handler(RedisOperations redis, IOptions<ProducersSettings> producersOptions)
        {
            _redis = redis;
            _producersOptions = producersOptions;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            
            var messages = _redis.ReadData().ToList();

            if (!messages.Any())
            {
                return Unit.Value;
            }
            
            var config = new ProducerConfig
            {
                BootstrapServers = _producersOptions.Value.ProducerForSendingMessagesToAnalyze.BootstrapServers
            };

            using var producer = new ProducerBuilder<Null, Message>(config)
                .SetValueSerializer(new EventSerializer<Message>())
                .Build();

            try
            {
                var tasks = messages
                    .Select(message =>
                        producer.ProduceAsync(
                            _producersOptions.Value.ProducerForSendingMessagesToAnalyze.Topic,
                            new Message<Null, Message> { Value = message },
                            cancellationToken))
                    .ToArray();
                
                await Task.WhenAll(tasks);
                
                await _redis.DeleteData(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке на анализ. "+ ex.Message);
            }

            return Unit.Value;
        }
    }
}