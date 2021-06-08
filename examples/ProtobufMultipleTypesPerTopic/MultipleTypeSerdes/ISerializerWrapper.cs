using System.Threading.Tasks;
using Confluent.Kafka;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    /// Wraps generic Protobuf serializer
    /// </summary>
    public interface ISerializerWrapper
    {
        Task<byte[]> SerializeAsync(object data, SerializationContext context);
    }
}