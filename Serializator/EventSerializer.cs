using System.Text.Json;
using Confluent.Kafka;

namespace Serializator;

public class EventSerializer<T>: ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        var message = JsonSerializer.SerializeToUtf8Bytes(data);
        return message;
    }
}