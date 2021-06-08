using System.Threading.Tasks;
using Confluent.Kafka;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    /// Wraps generic AvroSerializer
    /// </summary>
    public interface ISerializerWrapper
    {
        Task<byte[]> SerializeAsync(object data, SerializationContext context);
    }
}