using Confluent.Kafka;
using Domain;
using MediatR;
using Microsoft.Extensions.Options;
using Options;
using Redis;
using Serializator;

namespace Application;

public static class HandleStreamMessageCommand
{
    public record Request(Message Message) : IRequest<Unit>;

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
            var config = new ProducerConfig
            {
                BootstrapServers = _producersOptions.Value.ProducerForSendingMessagesToAnalyze.BootstrapServers
            };
            
            using var producer = new ProducerBuilder<Null, Message>(config)
                .SetValueSerializer(new EventSerializer<Message>())
                .Build();
            
            try
            {
                await producer.ProduceAsync(
                    _producersOptions.Value.ProducerForSendingMessagesToAnalyze.Topic, 
                    new Message<Null, Message>{Value = request.Message},
                    cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке сообщения на анализ. " + ex.Message);
                _redis.SaveFailedMessage(request.Message);
            }
            
            return Unit.Value;
        }
    }
}