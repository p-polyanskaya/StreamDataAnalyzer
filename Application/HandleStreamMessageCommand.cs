using Confluent.Kafka;
using Domain;
using MediatR;
using Redis;
using Serializator;

namespace Application;

public static class HandleStreamMessageCommand
{
    public record Request(Message Message) : IRequest<Unit>;

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly RedisOperations _redis; 
        public Handler(RedisOperations redis)
        {
            _redis = redis;
        }
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9093"
            };
            
            using var producer = new ProducerBuilder<Null, Message>(config)
                .SetValueSerializer(new EventSerializer<Message>())
                .Build();
            try
            {
                //await producer.ProduceAsync("test2-topic", request.Message, cancellationToken);
            }
            catch (Exception)
            {
                _redis.SaveBigData(request.Message);
            }
            
            return Unit.Value;
        }
    }
}