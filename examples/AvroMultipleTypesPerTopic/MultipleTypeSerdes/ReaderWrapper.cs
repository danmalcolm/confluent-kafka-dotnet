using Avro.IO;
using Avro.Specific;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
{
    internal class ReaderWrapper<T> : IReaderWrapper
    {
        private readonly SpecificReader<T> reader;

        public ReaderWrapper(Avro.Schema writerSchema, Avro.Schema readerSchema)
        {
            reader = new SpecificReader<T>(writerSchema, readerSchema);
        }

        public object Read(BinaryDecoder decoder) => reader.Read(default, decoder);
    }
}