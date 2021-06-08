using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
{
    internal class SerializerWrapper<T> : ISerializerWrapper
    {
        private AvroSerializer<T> inner;

        public SerializerWrapper(AvroSerializer<T> inner)
        {
            this.inner = inner;
        }

        public async Task<byte[]> SerializeAsync(object data, SerializationContext context)
        {
            return await inner.SerializeAsync((T)data, context).ConfigureAwait(false);
        }
    }
}