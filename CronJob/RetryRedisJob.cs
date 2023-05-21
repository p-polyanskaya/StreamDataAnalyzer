using Application;
using MediatR;

namespace CronJob;

public class RetryRedisJob
{
    private readonly IMediator _mediator;

    public RetryRedisJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute()
    {
        try
        {
            var request = new RetryFailedStreamMessageHandlingCommand.Request();
            await _mediator.Send(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка в RetryRedisJob. " + ex.Message);
        }
    }
}