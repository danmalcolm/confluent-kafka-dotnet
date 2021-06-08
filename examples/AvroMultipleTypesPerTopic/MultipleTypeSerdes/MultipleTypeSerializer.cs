using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    ///     (async) Avro serializer. Use this serializer with GenericRecord,
    ///     types generated using the avrogen.exe tool.
    /// </summary>
    public class MultipleTypeSerializer : IAsyncSerializer<object>
    {
        private readonly MultipleTypeConfig typeConfig;
        private readonly ISchemaRegistryClient schemaRegistryClient;
        private readonly AvroSerializerConfig serializerConfig;
        private readonly Dictionary<string, ISerializerWrapper> serializers = new Dictionary<string, ISerializerWrapper>();

        public MultipleTypeSerializer(MultipleTypeConfig typeConfig, 
            ISchemaRegistryClient schemaRegistryClient, 
            AvroSerializerConfig serializerConfig = null)
        {
            this.typeConfig = typeConfig;
            this.schemaRegistryClient = schemaRegistryClient;
            this.serializerConfig = serializerConfig;
            InitialiseSerializers();
        }

        private void InitialiseSerializers()
        {
            foreach (var typeInfo in typeConfig.Types)
            {
                var serializer = typeInfo.CreateSerializer(schemaRegistryClient, serializerConfig);
                serializers[typeInfo.ReaderSchema.Fullname] = serializer;
            }
        }
        
        public async Task<byte[]> SerializeAsync(object data, SerializationContext context)
        {
            var record = data as ISpecificRecord;
            if (record is null)
            {
                throw new ArgumentException($"Object being serialized is not an instance of {nameof(ISpecificRecord)}. This serializer only serializes types generated using the avrogen.exe tool.", nameof(data));
            }
            string fullName = record.Schema.Fullname;
            if (!serializers.TryGetValue(fullName, out var serializer))
            {
                throw new ArgumentException($"Unexpected type {fullName}. All types to be serialized should be registered in the {nameof(MultipleTypeConfig)} that is supplied to {nameof(MultipleTypeSerializer)}", nameof(data));
            }
            return await serializer.SerializeAsync(data, context);
        }
    }
}