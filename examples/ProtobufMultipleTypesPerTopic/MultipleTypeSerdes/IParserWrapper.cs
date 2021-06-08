using System.IO;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    /// Wraps generic Protobuf parser
    /// </summary>
    public interface IParserWrapper
    {
        object Read(Stream stream);
    }
}