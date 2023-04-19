using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

namespace Serializator;

public class EventDeserializer<T>: IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        return JsonSerializer.Deserialize<T>(data, options)!;
    }
}