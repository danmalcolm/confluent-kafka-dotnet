using System;
using System.Collections.Generic;
using System.Linq;
using Avro;
using Avro.Specific;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
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
        /// This must be a type generated using the avrogen.exe tool.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readerSchema">The Avro schema used to read the object (available via the generated _SCHEMA field</param>
        /// <returns></returns>
        public MultipleTypeConfigBuilder AddType<T>(Schema readerSchema)
            where T : ISpecificRecord
        {
            if (readerSchema is null)
            {
                throw new ArgumentNullException(nameof(readerSchema));
            }

            if (types.Any(x => x.ReaderSchema.Fullname == readerSchema.Fullname))
            {
                throw new ArgumentException($"A type based on schema with the full name \"{readerSchema.Fullname}\" has already been added");
            }
            var messageType = typeof(T);
            var mapping = new MultipleTypeInfo<T>(messageType, readerSchema);
            types.Add(mapping);
            return this;
        }

        public MultipleTypeConfig Build()
        {
            return new MultipleTypeConfig(types.ToArray());
        }
    }
}