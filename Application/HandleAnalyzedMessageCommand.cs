using Confluent.Kafka;
using Domain;
using MediatR;
using Microsoft.Extensions.Options;
using Options;
using Redis;
using Serializator;

namespace Application;

public static class HandleAnalyzedMessageCommand
{
    public record Request(IReadOnlyCollection<AnalysisResult> Messages) : IRequest<Unit>;

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
                BootstrapServers = _producersOptions.Value.ProducerForAnalyzedMessages.BootstrapServers
            };
            
            using var producer = new ProducerBuilder<Null, AnalysisResult>(config)
                .SetValueSerializer(new EventSerializer<AnalysisResult>())
                .Build();
            
            try
            {
                var tasks = request.Messages
                    .Select(message =>
                        producer.ProduceAsync(
                            _producersOptions.Value.ProducerForAnalyzedMessages.Topic,
                            new Message<Null, AnalysisResult> { Value = message },
                            cancellationToken))
                    .ToArray();
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                
            }
            
            return Unit.Value;
        }
    }
}