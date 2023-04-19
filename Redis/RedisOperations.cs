using Application;
using Confluent.Kafka;
using Domain;
using Serializator;

namespace Redis;

public class RedisOperations
{
    public void SaveBigData(Message message)
    {
        var cache = RedisConnectorHelper.Connection.GetDatabase();
        var messageValue = new EventSerializer<Message>().Serialize(message, new SerializationContext());
        cache.StringSet(message.Id.ToString(), messageValue);
    }
    
    public void ReadData()  
    {  
        var cache = RedisConnectorHelper.Connection.GetDatabase();  
        var devicesCount = 10000;  
        Console.WriteLine();
        for (int i = 0; i < devicesCount; i++)  
        {  
            var value = cache.StringGet($"Device_Status:{1}");
            Console.WriteLine($"Valor={value}");  
        }  
    } 
}