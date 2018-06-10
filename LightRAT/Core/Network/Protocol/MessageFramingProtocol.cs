using System;
using System.IO;
using System.Net;

namespace LightRAT.Core.Network.Protocol
{
    public class MessageFramingProtocol
    {
        private readonly int _maxBufferLength;

        private byte[] buffer;
        private byte[] bufferLength = BitConverter.GetBytes(sizeof(int));
        private int receivedData;
        private ReceivingMode receivingMode = ReceivingMode.Header;

        public MessageFramingProtocol(int maxBufferLength)
        {
            _maxBufferLength = maxBufferLength;
        }

        public event Action<byte[]> DataReceivedEvent;

        public void Read(byte[] data)
        {
            byte[] readingBufferHolder = null;
            var dataLength = 0;

            if (receivingMode == ReceivingMode.Header)
            {
                readingBufferHolder = bufferLength;
                dataLength = bufferLength.Length;
            }
            else
            {
                readingBufferHolder = buffer;

                var remainingDataLength = buffer.Length - receivedData;

                // we need to know if the incoming data is smaller than the needed data or not, if so we will read it all.
                dataLength = Math.Min(remainingDataLength, data.Length);
            }

            // if the buffer is null that means we will read the header (the first of the packet)
            // or if the receieved data doesn't equal the desired amout, we will also read from the start beacuse that means we received new data
            // if not we will read after the header
            var readingOffset = buffer == null || receivedData != data.Length && receivedData > 0
                ? 0
                : bufferLength.Length;

            // to skip the header while reading the first packet
            dataLength = buffer != null && receivedData == 0 ? data.Length - bufferLength.Length : dataLength;

            using (var bufferMemoryStream = new MemoryStream(readingBufferHolder, receivedData, dataLength)
            ) // to write data starting from the last index
            using (var dataMemoryStream = new MemoryStream(data, readingOffset, dataLength, false))
            {
                dataMemoryStream.WriteTo(bufferMemoryStream);
            }

            ReadCompeleted(dataLength, ref data);
        }

        private void ReadCompeleted(int dataLength, ref byte[] data)
        {
            if (receivingMode == ReceivingMode.Header)
            {
                var length = BitConverter.ToInt32(bufferLength, 0);

                if (length < 0)
                    throw new ProtocolViolationException("length cannot be less than zero");

                if (length > _maxBufferLength)
                    throw new ProtocolViolationException("the data length is greater than the maximum value.");

                buffer = new byte[length];
                receivingMode = ReceivingMode.Packet;
                Read(data);
            }
            else
            {
                receivedData += dataLength;

                if (receivedData == buffer.Length)
                {
                    DataReceivedEvent(buffer);
                    Reset();
                }
            }
        }

        private void Reset()
        {
            buffer = null;
            bufferLength = BitConverter.GetBytes(sizeof(int));
            receivingMode = ReceivingMode.Header;
            receivedData = 0;
        }

        public static byte[] Frame(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            var lengthInBytes = BitConverter.GetBytes(data.Length);
            var framedData = new byte[lengthInBytes.Length + data.Length];
            using (var frameDataMS = new MemoryStream(framedData))
            {
                using (var lengthMS = new MemoryStream(lengthInBytes))
                {
                    lengthMS.WriteTo(frameDataMS);
                }

                using (var dataMS = new MemoryStream(data))
                {
                    dataMS.WriteTo(frameDataMS);
                }
            }

            return framedData;
        }
    }
}