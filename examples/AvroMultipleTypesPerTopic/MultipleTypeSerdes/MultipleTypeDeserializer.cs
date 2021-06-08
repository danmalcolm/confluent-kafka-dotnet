using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avro.IO;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Schema = Avro.Schema;

namespace AvroMultipleTypesPerTopic.MultipleTypeSerdes
{
    /// <summary>
    ///     (async) Avro deserializer. Use this deserializer to deserialize
    ///     types generated using the avrogen.exe tool or one of the following 
    ///     primitive types: int, long, float, double, boolean, string, byte[].
    /// </summary>
    /// <remarks>
    ///     Serialization format:
    ///       byte 0:           Magic byte use to identify the protocol format.
    ///       bytes 1-4:        Unique global id of the Avro schema that was used for encoding (as registered in Confluent Schema Registry), big endian.
    ///       following bytes:  The serialized data.
    /// </remarks>
    public class MultipleTypeDeserializer : IAsyncDeserializer<object>
    {
        public const byte MagicByte = 0;
        private readonly ISchemaRegistryClient schemaRegistryClient;
        private readonly MultipleTypeConfig typeConfig;
        private readonly Dictionary<int, IReaderWrapper> messageReaders
            = new Dictionary<int, IReaderWrapper>();

        private readonly SemaphoreSlim deserializeMutex = new SemaphoreSlim(1);

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
                    
                    var readerWrapper = await GetReader(writerId);
                    return readerWrapper.Read(new BinaryDecoder(stream));
                }
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
            
        }

        private async Task<IReaderWrapper> GetReader(int writerId)
        {
            await deserializeMutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
            try
            {
                if (!messageReaders.TryGetValue(writerId, out var reader))
                {
                    if (messageReaders.Count > schemaRegistryClient.MaxCachedSchemas)
                    {
                        messageReaders.Clear();
                    }
                    var writerSchemaJson = await schemaRegistryClient.GetSchemaAsync(writerId)
                        .ConfigureAwait(continueOnCapturedContext: false);
                    var writerSchema = Schema.Parse(writerSchemaJson.SchemaString);
                    reader = typeConfig.CreateReader(writerSchema);
                    messageReaders[writerId] = reader;
                }
                return reader;
            }
            finally
            {
                deserializeMutex.Release();
            }
        }
    }
}