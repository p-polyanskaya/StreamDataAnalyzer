using Application;
using MediatR;

namespace CronJob;

public class RetryPostgresJob
{
    private readonly IMediator _mediator;

    public RetryPostgresJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Execute()
    {
        try
        {
            var request = new RetryFailedPostgresCommand.Request();
            await _mediator.Send(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка в RetryPostgresJob. " + ex.Message);
        }
    }
}