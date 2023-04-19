using StackExchange.Redis;

namespace Application;

public class RedisConnectorHelper
{
    static RedisConnectorHelper()
    {
        RedisConnectorHelper.lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("localhost:6379");
        });
    }

    private static Lazy<ConnectionMultiplexer> lazyConnection;

    public static ConnectionMultiplexer Connection
    {
        get { return lazyConnection.Value; }
    }
}