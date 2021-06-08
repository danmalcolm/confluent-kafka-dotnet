using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    /// Creates MultipleTypes configuration object used by MultipleTypeDeserializer, which specifies
    /// supported message value types
    /// </summary>
    public class MultipleTypeConfigBuilder
    {
        private readonly List<MultipleTypeInfo> types = new List<MultipleTypeInfo>();

        /// <summary>
        /// Adds details about a type of message that can be deserialized by MultipleTypeDeserializer.
        /// This must be a type generated from a .proto file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MultipleTypeConfigBuilder AddType<T>()
            where T : class, IMessage<T>, new()
        {
            var message = new T();
            return AddType<T>(message.Descriptor);
        }

        /// <summary>
        /// Adds details about a type of message that can be deserialized by MultipleTypeDeserializer.
        /// This must be a type generated from a .proto file.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MultipleTypeConfigBuilder AddType<T>(MessageDescriptor descriptor)
            where T : class, IMessage<T>, new()
        {
            if (types.Any(x => x.MessageType == typeof(T)))
            {
                throw new ArgumentException($"A type based on \"{typeof(T).FullName}\" has already been added");
            }
            if (descriptor.ClrType != typeof(T))
            {
                throw new ArgumentException($"Descriptor CLR Type ({descriptor.ClrType}) does not match message type", nameof(descriptor));
            }
            var messageType = typeof(T);
            var mapping = new MultipleTypeInfo<T>(messageType, descriptor);
            types.Add(mapping);
            return this;
        }

        public MultipleTypeConfig Build()
        {
            return new MultipleTypeConfig(types.ToArray());
        }
    }
}