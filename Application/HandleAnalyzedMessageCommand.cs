using Confluent.Kafka;
using Domain;
using MediatR;
using Microsoft.Extensions.Options;
using Options;
using Postgres;
using Serializator;

namespace Application;

public static class HandleAnalyzedMessageCommand
{
    public record Request(AnalysisResult AnalysisResult) : IRequest<Unit>;

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly FailedMessagesRepository _failedMessagesRepository;
        private readonly IOptions<ProducersSettings> _producersOptions;
        
        public Handler(FailedMessagesRepository failedMessagesRepository, IOptions<ProducersSettings> producersOptions)
        {
            _failedMessagesRepository = failedMessagesRepository;
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
                await producer.ProduceAsync(
                    _producersOptions.Value.ProducerForAnalyzedMessages.Topic,
                    new Message<Null, AnalysisResult> { Value = request.AnalysisResult },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке проанализированного сообщения. " + ex.Message + ex.StackTrace);

                var failedMessage = new FailedMessage
                {
                    Id = request.AnalysisResult.Message.Id,
                    Topic = _producersOptions.Value.ProducerForAnalyzedMessages.Topic,
                    AnalysisResult = request.AnalysisResult
                };
                await _failedMessagesRepository.Insert(failedMessage);
            }
            
            return Unit.Value;
        }
    }
}