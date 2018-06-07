using System;
using System.IO;
using System.Net;


namespace LightRAT.Core.Network.Protocol
{
    public class MessageFramingProtocol
    {
        private byte[] bufferLength = BitConverter.GetBytes(sizeof(int));
        private ReceivingMode receivingMode = ReceivingMode.Header;
        private byte[] buffer;
        private int _maxBufferLength;
        private int receivedData = 0;

        public event Action<byte[]> DataReceivedEvent;

        public MessageFramingProtocol(int maxBufferLength)
        {
            _maxBufferLength = maxBufferLength;
        }

        public void Read(byte[] data)
        {
            byte[] readingBufferHolder = null;
            int dataLength = 0;

            if (receivingMode == ReceivingMode.Header)
            {
                readingBufferHolder = bufferLength;
                dataLength = bufferLength.Length;
            }
            if (receivingMode == ReceivingMode.Packet)
            {
                readingBufferHolder = buffer;

                int remainingDataLength = buffer.Length - receivedData;
                
                // we need to know if the incoming data is smaller than the needed data or not, if so we will read it all.
                dataLength = Math.Min(remainingDataLength, data.Length);
            }

            // if the buffer is null that means we read the header (the first of the packet)
            // or if the receieved data doesn't equal the desired amout, we read from the start beacuse that means we received new data
            // if not we read after the header
            int readingOffset = (buffer == null || (receivedData != data.Length && receivedData > 0)) ? 0 : bufferLength.Length;

            // Array.Copy(data, readingOffset, readingBufferHolder, 0, dataLength);
            using (var bufferMemoryStream = new MemoryStream(readingBufferHolder))
                using (var dataMemoryStream = new MemoryStream(data, readingOffset, dataLength))
                    dataMemoryStream.CopyTo(bufferMemoryStream);

            ReadCompeleted(dataLength, data);
        }
        private void ReadCompeleted(int dataLength, byte[] data)
        {

            if (receivingMode == ReceivingMode.Header)
            {
                int length = BitConverter.ToInt32(bufferLength, 0);

                if (length < 0)
                    throw new ProtocolViolationException("length cannot be less than zero");

                if (length > _maxBufferLength)
                    throw new ProtocolViolationException("the data length is greater than the maximum value.");

                buffer = new byte[length];
                receivingMode = ReceivingMode.Packet;
                Read(data);
            }

            if (receivingMode == ReceivingMode.Packet)
            {
                if (receivedData != buffer.Length)
                {
                    receivedData += dataLength;
                }
                else
                {
                    DataReceivedEvent(buffer);
                    Reset();
                }
            }
        }

        public void Reset()
        {
            buffer = null;
            bufferLength = BitConverter.GetBytes(sizeof(int));
            receivingMode = ReceivingMode.Header;
            receivedData = 0;
        }
        public static byte[] Frame(byte[] data)
        {
            byte[] lengthInBytes = BitConverter.GetBytes(data.Length);
            byte[] framedData = new byte[lengthInBytes.Length + data.Length];
            Array.Copy(lengthInBytes, framedData, lengthInBytes.Length);
            Array.Copy(data, 0, framedData, lengthInBytes.Length, data.Length);
            return framedData;
        }
    }
}
