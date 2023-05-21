using Application;
using Grpc.Core;
using MediatR;

namespace GrpcServices;

public class StreamGrpcService : DataStreamer.DataStreamerBase
{
    private readonly IMediator _mediator;

    public StreamGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Response> SendStreamData(IAsyncStreamReader<Request> requestStream,
        ServerCallContext context)
    {
        try
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                try
                {
                    var domainMessage = ToDomainMessage(request.Message);
                    var command = new HandleStreamMessageCommand.Request(domainMessage);
                    await _mediator.Send(command, context.CancellationToken);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Ошибка при отправке сообщения на анализ. " + ex.Message);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("Ошибка в requestStream. " + ex.Message);
        }

        return new Response();
    }


    public override async Task<Response> SendData(Request request, ServerCallContext context)
    {
        try
        {
            var domainMessage = ToDomainMessage(request.Message);
            var command = new HandleStreamMessageCommand.Request(domainMessage);
            await _mediator.Send(command, context.CancellationToken);

        }
        catch(Exception ex)
        {
            Console.WriteLine("Ошибка при отправке сообщения на анализ. " + ex.Message);
        }
        
        return new Response();
    }

    private static Domain.Message ToDomainMessage(Message message)
    {
        return new Domain.Message(
            new Guid(message.Id),
            message.Author,
            message.Text,
            message.TimeOfMessage.ToDateTime(),
            message.Source
        );
    }
}