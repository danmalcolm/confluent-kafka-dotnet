using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.SchemaRegistry;
using Google.Protobuf.Reflection;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    /// Configuration used by MultipleTypeDeserializer to specify supported message value types
    /// </summary>
    public class MultipleTypeConfig
    {
        private readonly MultipleTypeInfo[] types;

        internal MultipleTypeConfig(MultipleTypeInfo[] types)
        {
            this.types = types;
        }
        
        public IParserWrapper CreateReader(FileDescriptor descriptor)
        {
            var fullName = descriptor.MessageTypes[0].FullName;
            var type = types.SingleOrDefault(x => x.MessageType.FullName.Equals(fullName, StringComparison.OrdinalIgnoreCase));
            return type.CreateParser();
        }

        public IEnumerable<MultipleTypeInfo> Types => types.AsEnumerable();
    }
}