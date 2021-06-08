using System.IO;
using Google.Protobuf;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    internal class ParserWrapper<T> : IParserWrapper where T : class, IMessage<T>, new()
    {
        private readonly MessageParser<T> _parser;

        public ParserWrapper(MessageParser<T> parser)
        {
            _parser = parser;
        }

        public object Read(Stream stream) => _parser.ParseFrom(stream);
    }
}