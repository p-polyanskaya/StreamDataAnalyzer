using Grpc.Core;

namespace GrpcServices;

public class StreamGrpcService: DataStreamer.DataStreamerBase
{
    public override async Task<Response> SendStreamData(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
    {
        await foreach(Request request in requestStream.ReadAllAsync())
        {
            Console.WriteLine("aaaaaaaaa "+request.Message.Id);
            await Task.Delay(600000);
            Console.WriteLine("hiiiii "+request.Message.Id);
        }

        return new Response();
    }
}