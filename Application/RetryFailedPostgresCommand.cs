using Confluent.Kafka;
using Domain;
using MediatR;
using Microsoft.Extensions.Options;
using Options;
using Postgres;
using Serializator;

namespace Application;

public static class RetryFailedPostgresCommand
{
    public record Request() : IRequest<Unit>;

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly IOptions<ProducersSettings> _producersOptions;
        private readonly FailedMessagesRepository _failedMessagesRepository;

        public Handler(IOptions<ProducersSettings> producersOptions, FailedMessagesRepository failedMessagesRepository)
        {
            _producersOptions = producersOptions;
            _failedMessagesRepository = failedMessagesRepository;
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var messages = await _failedMessagesRepository.Get();

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
                            message.Topic,
                            new Message<Null, Message> { Value = message.AnalysisResult.Message},
                            cancellationToken))
                    .ToArray();

                await Task.WhenAll(tasks);
                
                await _failedMessagesRepository.Delete(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке проанализированных сообщений. " +  ex.Message);
            }

            return Unit.Value;
        }
    }
}