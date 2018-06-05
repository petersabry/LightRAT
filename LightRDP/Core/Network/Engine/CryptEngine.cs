using System;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace LightRDP.Core.Network.Engine
{
    public static class CryptEngine
    {
        public static byte[] Compress(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            using (var compressedDataStream = new MemoryStream())
            {
                using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    using (var gzipStream = new GZipStream(compressedDataStream, CompressionMode.Compress))
                    {
                        dataStream.CopyTo(gzipStream);
                    }
                }
                return compressedDataStream.ToArray();
            }
        }
        public static string Decompress(byte[] data)
        {
            if(data == null || data.Length == 0)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            using (var compressedDataStream = new MemoryStream(data))
            {
                using (var gzipStream = new GZipStream(compressedDataStream, CompressionMode.Decompress))
                {
                    using (var decompressedDataStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(decompressedDataStream);
                        return Encoding.UTF8.GetString(decompressedDataStream.ToArray());
                    }
                }
            }
        }
    }
}
