using Avro.IO;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    /// Wraps generic Avro SpecificReader
    /// </summary>
    public interface IReaderWrapper
    {
        object Read(BinaryDecoder decoder);
    }
}