using Confluent.Kafka;
using Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Options;
using Serializator;

namespace Redis;

public class RedisOperations
{
    private readonly RedisConnectorHelper _redisConnectorHelper;
    private readonly IOptions<RedisConnection> _redisOptions;

    public RedisOperations(RedisConnectorHelper redisConnectorHelper, IOptions<RedisConnection> redisOptions)
    {
        _redisConnectorHelper = redisConnectorHelper;
        _redisOptions = redisOptions;
    }
    
    public void SaveFailedMessage(Message message)
    {
        var cache = _redisConnectorHelper.Connection.GetDatabase();
        var messageValue = new EventSerializer<Message>().Serialize(message, new SerializationContext());
        cache.StringSet(message.Id.ToString(), messageValue);
    }
    
    public IEnumerable<Message> ReadData()
    {
        var cache = _redisConnectorHelper.Connection.GetDatabase();
        var server = _redisConnectorHelper.Connection.GetServer(_redisOptions.Value.Host, Convert.ToInt32(_redisOptions.Value.Port));
        
        var redisKeys = server.Keys().ToList().Take(100);
        var messages = new List<Message>();
        foreach (var key in redisKeys)
        {
            var message = cache.StringGet(key);
            if (message.HasValue)
            {
                var messageValue = JsonConvert.DeserializeObject<Message>(message);
                messages.Add(messageValue);
            }
        }

        return messages;
    } 
    
    public async Task DeleteData(IEnumerable<Message> messages)
    {
        var ids = messages.Select(message => message.Id.ToString()).ToArray();
        var cache = _redisConnectorHelper.Connection.GetDatabase();
        foreach (var id in ids)
        {
            await cache.KeyDeleteAsync(id);
        }
    }
}