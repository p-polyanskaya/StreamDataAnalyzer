using Microsoft.Extensions.Options;
using Options;
using StackExchange.Redis;

namespace Redis;

public class RedisConnectorHelper
{
    private readonly IOptions<RedisConnection> _redisOptions;
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

    public RedisConnectorHelper(IOptions<RedisConnection> redisOptions)
    {
        _redisOptions = redisOptions;
        _lazyConnection = new Lazy<ConnectionMultiplexer>(GetConnectionMultiplexer);
    }
    
    private ConnectionMultiplexer GetConnectionMultiplexer()
    {
        var options = ConfigurationOptions.Parse(_redisOptions.Value.Url);
        options.ConnectRetry = 5;
        options.AllowAdmin = true;
        return ConnectionMultiplexer.Connect(options);
    }
    
    public ConnectionMultiplexer Connection => _lazyConnection.Value;
}