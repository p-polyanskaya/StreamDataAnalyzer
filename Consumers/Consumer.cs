using Application;
using Confluent.Kafka;
using Domain;
using Endpoint;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Options;
using Serializator;

namespace Consumers;

public class Consumer : BackgroundService
{
    private readonly IOptions<ConsumersSettings> _consumerSettings;
    private readonly ConsumerBuilder<Ignore, Message> _builder;
    private readonly IServiceProvider _serviceProvider;
    private readonly PredictionEnginePool<ModelInput, ModelOutput> _enginePool;

    public Consumer(IOptions<ConsumersSettings> consumerSettings, IServiceProvider serviceProvider,
        PredictionEnginePool<ModelInput, ModelOutput> enginePool)
    {
        _consumerSettings = consumerSettings;
        _serviceProvider = serviceProvider;
        _enginePool = enginePool;
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
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);

                var topicOfNews = GetTopicOfNews(consumeResult.Message.Value.Text);
                var analysisResult = new AnalysisResult(consumeResult.Message.Value, topicOfNews);
                var request = new HandleAnalyzedMessageCommand.Request(analysisResult);

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(request, stoppingToken);
                consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке проанализированного сообщения. " + ex.Message);
            }
        }
    }

    private string GetTopicOfNews(string textOfNews)
    {
        var modelInput = new ModelInput
        {
            Sentence = textOfNews
        };
        
        var result = _enginePool.Predict(modelName: "NewsRecommendation", modelInput);

        var topic = ((Article)(result.PredictedLabel)).ToString("G");
        
        return topic;
    }
    
    private enum Article
    {
        World = 1,
        Sports = 2,
        Business = 3,
        Tech = 4
    }
}