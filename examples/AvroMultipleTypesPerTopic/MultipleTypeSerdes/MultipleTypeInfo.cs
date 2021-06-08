using System;
using Avro.Specific;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Schema = Avro.Schema;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
{
    public abstract class MultipleTypeInfo
    {        
        public MultipleTypeInfo(Type messageType, Schema readerSchema)
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            ReaderSchema = readerSchema ?? throw new ArgumentNullException(nameof(readerSchema));
        }

        public Type MessageType { get; }

        public Schema ReaderSchema { get; }

        public abstract IReaderWrapper CreateReader(Schema writerSchema);

        public abstract ISerializerWrapper CreateSerializer(ISchemaRegistryClient schemaRegistryClient,
            AvroSerializerConfig serializerConfig);
    }

    public class MultipleTypeInfo<T> : MultipleTypeInfo
        where T : ISpecificRecord
    {
        public MultipleTypeInfo(Type messageType, Schema readerSchema) : base(messageType, readerSchema)
        {
        }

        public override IReaderWrapper CreateReader(Schema writerSchema)
        {
            return new ReaderWrapper<T>(writerSchema, ReaderSchema);
        }

        public override ISerializerWrapper CreateSerializer(ISchemaRegistryClient schemaRegistryClient, AvroSerializerConfig serializerConfig)
        {
            var inner = new AvroSerializer<T>(schemaRegistryClient, serializerConfig);
            return new SerializerWrapper<T>(inner);
        }
    }
}