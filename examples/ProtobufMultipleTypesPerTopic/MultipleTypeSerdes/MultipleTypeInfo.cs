using System;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    public abstract class MultipleTypeInfo
    {        
        public MultipleTypeInfo(Type messageType, MessageDescriptor descriptor)
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            Descriptor = descriptor;
        }

        public Type MessageType { get; }

        public MessageDescriptor Descriptor { get; }

        public abstract IParserWrapper CreateParser();

        public abstract ISerializerWrapper CreateSerializer(ISchemaRegistryClient schemaRegistryClient, ProtobufSerializerConfig serializerConfig);
    }

    public class MultipleTypeInfo<T> : MultipleTypeInfo
        where T : class, IMessage<T>, new()
    {
        public MultipleTypeInfo(Type messageType, MessageDescriptor descriptor) : base(messageType, descriptor)
        {
            
        }

        public override IParserWrapper CreateParser()
        {
            var parser = new MessageParser<T>(() => new T());
            return new ParserWrapper<T>(parser);
        }

        public override ISerializerWrapper CreateSerializer(ISchemaRegistryClient schemaRegistryClient, ProtobufSerializerConfig serializerConfig)
        {
            var inner = new ProtobufSerializer<T>(schemaRegistryClient, serializerConfig);
            return new SerializerWrapper<T>(inner);
        }
    }
}