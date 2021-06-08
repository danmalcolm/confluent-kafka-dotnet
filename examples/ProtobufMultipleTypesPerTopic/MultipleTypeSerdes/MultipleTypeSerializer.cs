using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    ///     (async) Protobuf serializer. Use this serializer with GenericRecord,
    ///     types generated using the Protobufgen.exe tool.
    /// </summary>
    public class MultipleTypeSerializer : IAsyncSerializer<object>
    {
        private readonly MultipleTypeConfig typeConfig;
        private readonly ISchemaRegistryClient schemaRegistryClient;
        private readonly ProtobufSerializerConfig serializerConfig;
        private readonly Dictionary<Type, ISerializerWrapper> serializers = new Dictionary<Type, ISerializerWrapper>();

        public MultipleTypeSerializer(MultipleTypeConfig typeConfig, 
            ISchemaRegistryClient schemaRegistryClient, 
            ProtobufSerializerConfig serializerConfig = null)
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
                serializers[typeInfo.MessageType] = serializer;
            }
        }
        
        public async Task<byte[]> SerializeAsync(object data, SerializationContext context)
        {
            var message = data as IMessage;
            if (message is null)
            {
                throw new ArgumentException($"Object being serialized is not an instance of {nameof(IMessage)}. This serializer only serializes types generated using the Protobufgen.exe tool.", nameof(data));
            }
            var messageType = message.Descriptor.ClrType;
            if (!serializers.TryGetValue(messageType, out var serializer))
            {
                throw new ArgumentException($"Unexpected type {messageType.FullName}. All types to be serialized should be registered in the {nameof(MultipleTypeConfig)} that is supplied to {nameof(MultipleTypeSerializer)}", nameof(data));
            }
            return await serializer.SerializeAsync(data, context);
        }
    }
}