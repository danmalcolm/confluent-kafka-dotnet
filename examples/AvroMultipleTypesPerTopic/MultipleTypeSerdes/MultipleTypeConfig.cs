using System.Collections.Generic;
using System.Linq;
using Avro;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
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
        
        public IReaderWrapper CreateReader(Schema writerSchema)
        {
            var type = types.SingleOrDefault(x => x.ReaderSchema.Fullname == writerSchema.Fullname);
            return type.CreateReader(writerSchema);
        }

        public IEnumerable<MultipleTypeInfo> Types => types.AsEnumerable();
    }
}