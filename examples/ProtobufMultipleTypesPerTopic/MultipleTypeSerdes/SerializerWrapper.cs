using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    internal class SerializerWrapper<T> : ISerializerWrapper
    where T : class, IMessage<T>, new()
    {
        private readonly ProtobufSerializer<T> inner;

        public SerializerWrapper(ProtobufSerializer<T> inner)
        {
            this.inner = inner;
        }

        public async Task<byte[]> SerializeAsync(object data, SerializationContext context)
        {
            return await inner.SerializeAsync((T)data, context).ConfigureAwait(false);
        }
    }
}