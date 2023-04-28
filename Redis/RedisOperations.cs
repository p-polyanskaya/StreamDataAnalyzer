using Confluent.Kafka;
using Domain;
using Serializator;

namespace Redis;

public class RedisOperations
{
    private readonly RedisConnectorHelper _redisConnectorHelper;
    public RedisOperations(RedisConnectorHelper redisConnectorHelper)
    {
        _redisConnectorHelper = redisConnectorHelper;
    }
    
    public void SaveBigData(Message message)
    {
        var cache = _redisConnectorHelper.Connection.GetDatabase();
        var messageValue = new EventSerializer<Message>().Serialize(message, new SerializationContext());
        cache.StringSet(message.Id.ToString(), messageValue);
    }
    
    public async Task<IEnumerable<Message>> ReadData()  
    {  
        var cache = _redisConnectorHelper.Connection.GetDatabase();
        return null;
    } 
    
    public async Task DeleteData(IEnumerable<Message> messages)
    {
        var ids = messages.Select(message => message.Id.ToString());
        var cache = _redisConnectorHelper.Connection.GetDatabase();
    }
}