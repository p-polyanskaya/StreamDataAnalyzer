using Microsoft.Extensions.Options;
using Options;
using StackExchange.Redis;

namespace Redis;

public class RedisConnectorHelper
{   
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

    public RedisConnectorHelper(IOptions<RedisConnection> redisOptions)
    {
        _lazyConnection = new Lazy<ConnectionMultiplexer>(() => 
            ConnectionMultiplexer.Connect(redisOptions.Value.Url));
    }
    
    public ConnectionMultiplexer Connection => _lazyConnection.Value;
}