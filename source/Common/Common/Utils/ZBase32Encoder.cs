using Karambolo.Common;
using System;

namespace AspNetSkeleton.Common.Utils
{
    // https://github.com/denxc/ZBase32Encoder/blob/master/ZBase32Encoder/ZBase32Encoder/ZBase32Encoder.cs
    public static class ZBase32Encoder
    {
        const string encodingTable = "ybndrfg8ejkmcpqxot1uwisza345h769";

        static readonly byte[] decodingTable = new byte[128];

        static ZBase32Encoder()
        {
            for (var i = 0; i < decodingTable.Length; ++i)
                decodingTable[i] = byte.MaxValue;

            for (var i = 0; i < encodingTable.Length; ++i)
                decodingTable[encodingTable[i]] = (byte)i;
        }

        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var result = new char[(int)Math.Ceiling(data.Length * 8.0 / 5.0)];
            var n = 0;

            for (var i = 0; i < data.Length; i += 5)
            {
                var byteCount = Math.Min(5, data.Length - i);

                ulong buffer = 0;
                for (var j = 0; j < byteCount; ++j)
                    buffer = (buffer << 8) | data[i + j];

                var bitCount = byteCount * 8;
                while (bitCount > 0)
                {
                    var index =
                        bitCount >= 5 ?
                        (int)(buffer >> (bitCount - 5)) & 0x1f :
                        (int)(buffer & (ulong)(0x1f >> (5 - bitCount))) << (5 - bitCount);

                    result[n++] = encodingTable[index];
                    bitCount -= 5;
                }
            }

            return new string(result);
        }

        public static byte[] Decode(string data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data == string.Empty)
                return ArrayUtils.Empty<byte>();

            var result = new byte[(int)Math.Ceiling(data.Length * 5.0 / 8.0)];

            var n = 0;
            var index = new int[8];
            for (var i = 0; i < data.Length;)
            {
                i = CreateIndexByOctetAndMovePosition(ref data, i, ref index);

                var shortByteCount = 0;
                ulong buffer = 0;
                for (var j = 0; j < 8 && index[j] != -1; ++j)
                {
                    buffer = (buffer << 5) | (ulong)(long)(decodingTable[index[j]] & 0x1f);
                    shortByteCount++;
                }

                var bitCount = shortByteCount * 5;
                while (bitCount >= 8)
                {
                    result[n++] = (byte)((buffer >> (bitCount - 8)) & 0xff);
                    bitCount -= 8;
                }
            }

            return result;
        }

        static int CreateIndexByOctetAndMovePosition(ref string data, int currentPosition, ref int[] index)
        {
            var j = 0;
            while (j < 8)
            {
                if (currentPosition >= data.Length)
                {
                    index[j++] = -1;
                    continue;
                }

                if (IgnoredSymbol(data[currentPosition]))
                {
                    currentPosition++;
                    continue;
                }

                index[j] = data[currentPosition];
                j++;
                currentPosition++;
            }

            return currentPosition;
        }

        static bool IgnoredSymbol(char checkedSymbol)
        {
            return checkedSymbol >= decodingTable.Length || decodingTable[checkedSymbol] == byte.MaxValue;
        }
    }
}
