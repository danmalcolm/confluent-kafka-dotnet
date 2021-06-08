using System;
using System.IO;

namespace ProtobufMultipleTypesPerTopic.MultipleTypeSerdes
{
    internal static class Utils
    {
        public static void WriteVarint(this Stream stream, uint value)
        {
            WriteUnsignedVarint(stream, (value << 1) ^ (value >> 31));
        }

        /// <remarks>
        ///     Inspired by: https://github.com/apache/kafka/blob/2.5/clients/src/main/java/org/apache/kafka/common/utils/ByteUtils.java#L284
        /// </remarks>
        public static void WriteUnsignedVarint(this Stream stream, uint value)
        {
            while ((value & 0xffffff80) != 0L)
            {
                byte b = (byte)((value & 0x7f) | 0x80);
                stream.WriteByte(b);
                value >>= 7;
            }
            stream.WriteByte((byte)value);
        }
        public static int ReadVarint(this Stream stream)
        {
            var value = ReadUnsignedVarint(stream);
            return (int)((value >> 1) ^ -(value & 1));
        }

        /// <remarks>
        ///     Inspired by: https://github.com/apache/kafka/blob/2.5/clients/src/main/java/org/apache/kafka/common/utils/ByteUtils.java#L142
        /// </remarks>
        public static uint ReadUnsignedVarint(this Stream stream)
        {
            int value = 0;
            int i = 0;
            int b;
            while (true)
            {
                b = stream.ReadByte();
                if (b == -1) throw new InvalidOperationException("Unexpected end of stream reading varint.");
                if ((b & 0x80) == 0) { break; }
                value |= (b & 0x7f) << i;
                i += 7;
                if (i > 28)
                {
                    throw new OverflowException($"Encoded varint is larger than uint.MaxValue");
                }
            }
            value |= b << i;
            return (uint)value;
        }
    }
}