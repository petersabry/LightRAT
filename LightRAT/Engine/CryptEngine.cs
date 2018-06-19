using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace LightRAT.Engine
{
    public static class CryptEngine
    {
        public static byte[] Compress(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            using (var compressedDataStream = new MemoryStream())
            {
                using (var dataStream = new MemoryStream(data))
                {
                    using (var gzipStream = new GZipStream(compressedDataStream, CompressionMode.Compress))
                    {
                        dataStream.CopyTo(gzipStream);
                    }
                }

                return compressedDataStream.ToArray();
            }
        }
        public static byte[] Decompress(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            using (var compressedDataStream = new MemoryStream(data))
            {
                using (var gzipStream = new GZipStream(compressedDataStream, CompressionMode.Decompress))
                {
                    using (var decompressedDataStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(decompressedDataStream);
                        return decompressedDataStream.ToArray();
                    }
                }
            }
        }
        #region Asynchronous Methods
        public static async Task<byte[]> CompressAsync(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            using (var compressedDataStream = new MemoryStream())
            {
                using (var dataStream = new MemoryStream(data))
                {
                    using (var gzipStream = new GZipStream(compressedDataStream, CompressionMode.Compress))
                    {
                        await dataStream.CopyToAsync(gzipStream);
                    }
                }

                return compressedDataStream.ToArray();
            }
        }
        public static async Task<byte[]> DecompressAsync(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            using (var compressedDataStream = new MemoryStream(data))
            {
                using (var gzipStream = new GZipStream(compressedDataStream, CompressionMode.Decompress))
                {
                    using (var decompressedDataStream = new MemoryStream())
                    {
                        await gzipStream.CopyToAsync(decompressedDataStream);
                        return decompressedDataStream.ToArray();
                    }
                }
            }
        }
        #endregion
    }
}