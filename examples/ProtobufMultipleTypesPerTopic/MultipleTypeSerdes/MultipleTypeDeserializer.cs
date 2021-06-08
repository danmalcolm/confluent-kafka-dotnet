using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <remarks>
    ///     Serialization format:
    ///       byte 0:           A magic byte that identifies this as a message with
    ///                         Confluent Platform framing.
    ///       bytes 1-4:        Unique global id of the Protobuf schema that was used
    ///                         for encoding (as registered in Confluent Schema Registry),
    ///                         big endian.
    ///       following bytes:  1. A size-prefixed array of indices that identify the
    ///                            specific message type in the schema (a given schema
    ///                            can contain many message types and they can be nested).
    ///                            Size and indices are unsigned varints. The common case
    ///                            where the message type is the first message in the
    ///                            schema (i.e. index data would be [1,0]) is encoded as
    ///                            a single 0 byte as an optimization.
    ///                         2. The protobuf serialized data.
    /// </remarks>
    public class MultipleTypeDeserializer : IAsyncDeserializer<object>
    {
        public const byte MagicByte = 0;
        private readonly ISchemaRegistryClient schemaRegistryClient;
        private readonly ProtobufDeserializerConfig config;
        private readonly MultipleTypeConfig typeConfig;
        private readonly Dictionary<int, IParserWrapper> parsers
            = new Dictionary<int, IParserWrapper>();

        private readonly SemaphoreSlim deserializeMutex = new SemaphoreSlim(1);
        private bool useDeprecatedFormat = false;

        /// <summary>
        ///     Initialize a new AvroDeserializer instance.
        /// </summary>
        /// <param name="schemaRegistryClient">
        ///     An implementation of ISchemaRegistryClient used for
        ///     communication with Confluent Schema Registry.
        /// </param>
        /// <param name="config">
        ///     Deserializer configuration properties (refer to 
        ///     <see cref="AvroDeserializerConfig" />).
        /// </param>
        public MultipleTypeDeserializer(MultipleTypeConfig typeConfig, ISchemaRegistryClient schemaRegistryClient)
        {
            this.typeConfig = typeConfig;
            this.schemaRegistryClient = schemaRegistryClient;
        }

        /// <summary>
        ///     Deserialize an object of one of the specified types from a byte array
        /// </summary>
        /// <param name="data">
        ///     The raw byte data to deserialize.
        /// </param>
        /// <param name="isNull">
        ///     True if this is a null value.
        /// </param>
        /// <param name="context">
        ///     Context relevant to the deserialize operation.
        /// </param>
        /// <returns>
        ///     A <see cref="System.Threading.Tasks.Task" /> that completes
        ///     with the deserialized value.
        /// </returns>
        public async Task<object> DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)
        {
            try
            {
                if (data.Length < 5)
                {
                    throw new InvalidDataException($"Expecting data framing of length 5 bytes or more but total data size is {data.Length} bytes");
                }

                using (var stream = new MemoryStream(data.ToArray()))
                using (var reader = new BinaryReader(stream))
                {
                    var magicByte = reader.ReadByte();
                    if (magicByte != MagicByte)
                    {
                        throw new InvalidDataException($"Expecting data with Confluent Schema Registry framing. Magic byte was {magicByte}, expecting {MagicByte}");
                    }
                    var writerId = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                    var parserWrapper = await GetParser(writerId);
                    // Read the index array length, then all of the indices. These are not
                    // needed, but parsing them is the easiest way to seek to the start of
                    // the serialized data because they are varints.
                    var indicesLength = useDeprecatedFormat ? (int)stream.ReadUnsignedVarint() : stream.ReadVarint();
                    for (int i = 0; i < indicesLength; ++i)
                    {
                        if (useDeprecatedFormat)
                        {
                            stream.ReadUnsignedVarint();
                        }
                        else
                        {
                            stream.ReadVarint();
                        }
                    }


                    return parserWrapper.Read(stream);
                }
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        ///     Deserialize an object of type <typeparamref name="T"/>
        ///     from a byte array.
        /// </summary>
        /// <param name="data">
        ///     The raw byte data to deserialize.
        /// </param>
        /// <param name="isNull">
        ///     True if this is a null value.
        /// </param>
        /// <param name="context">
        ///     Context relevant to the deserialize operation.
        /// </param>
        /// <returns>
        ///     A <see cref="System.Threading.Tasks.Task" /> that completes
        ///     with the deserialized value.
        /// </returns>
        public async Task<object> DeserializeAsync2(ReadOnlyMemory<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull) { return Task.FromResult<object>(null); }

            var array = data.ToArray();
            if (array.Length < 6)
            {
                throw new InvalidDataException($"Expecting data framing of length 6 bytes or more but total data size is {array.Length} bytes");
            }

            try
            {
                using (var stream = new MemoryStream(array))
                using (var reader = new BinaryReader(stream))
                {
                    var magicByte = reader.ReadByte();
                    if (magicByte != MagicByte)
                    {
                        throw new InvalidDataException($"Expecting message {context.Component.ToString()} with Confluent Schema Registry framing. Magic byte was {array[0]}, expecting {MagicByte}");
                    }

                    var schemaId = IPAddress.NetworkToHostOrder(reader.ReadInt32());
                    var parser = await GetParser(schemaId);
                    // Read the index array length, then all of the indices. These are not
                    // needed, but parsing them is the easiest way to seek to the start of
                    // the serialized data because they are varints.
                    var indicesLength = useDeprecatedFormat ? (int)stream.ReadUnsignedVarint() : stream.ReadVarint();
                    for (int i = 0; i < indicesLength; ++i)
                    {
                        stream.ReadVarint();
                    }

                    return Task.FromResult(parser.Read(stream));
                }
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }


        private async Task<IParserWrapper> GetParser(int schemaId)
        {
            await deserializeMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
            try
            {
                if (!parsers.TryGetValue(schemaId, out var parser))
                {
                    if (parsers.Count > schemaRegistryClient.MaxCachedSchemas)
                    {
                        parsers.Clear();
                    }

                    var registrySchema = await schemaRegistryClient.GetSchemaAsync(schemaId, "serialized")
                        .ConfigureAwait(continueOnCapturedContext: false);
                    var descriptor = ParseFileDescriptor(registrySchema);
                    parser = typeConfig.CreateReader(descriptor);
                    parsers[schemaId] = parser;
                }
                return parser;
            }
            finally
            {
                deserializeMutex.Release();
            }
        }

        private static FileDescriptor ParseFileDescriptor(Schema registrySchema)
        {
            var byteString = ByteString.FromBase64(registrySchema.SchemaString);
            var descriptors = FileDescriptor.BuildFromByteStrings(new[] {byteString});
            return descriptors[0];
        }
    }
}