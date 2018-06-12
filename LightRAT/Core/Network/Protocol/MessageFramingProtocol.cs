using System;
using System.IO;
using System.Net;


namespace LightRAT.Core.Network.Protocol
{
    public class MessageFramingProtocol
    {
        private readonly int _maxBufferLength;

        private byte[] _buffer;
        private byte[] _bufferLength = BitConverter.GetBytes(sizeof(int));
        private int _receivedData;
        private ReceivingMode _receivingMode = ReceivingMode.Header;
        private int _readingOffset;

        public MessageFramingProtocol(int maxBufferLength)
        {
            _maxBufferLength = maxBufferLength;
        }

        public event Action<byte[]> DataReceivedEvent;

        public void Read(byte[] data)
        {
            byte[] readingBufferHolder;
            int dataLength;

            if (_receivingMode == ReceivingMode.Header)
            {
                readingBufferHolder = _bufferLength;
                dataLength = _bufferLength.Length;
            }
            else
            {
                readingBufferHolder = _buffer;

                var remainingDataLength = _buffer.Length - _receivedData;

                // we need to know if the incoming data is smaller than the needed data or not, if so we will read it all.
                dataLength = Math.Min(remainingDataLength, data.Length);

                // this mainly used to skip the header
                if (dataLength != remainingDataLength)
                    dataLength -= _readingOffset;
            }

            using (var bufferMemoryStream = new MemoryStream(readingBufferHolder, _receivedData, dataLength))
                using (var dataMemoryStream = new MemoryStream(data, _readingOffset, dataLength, false))
                        dataMemoryStream.WriteTo(bufferMemoryStream);

            ReadCompeleted(dataLength, ref data);
        }

        private void ReadCompeleted(int dataLength, ref byte[] data)
        {
            if (_receivingMode == ReceivingMode.Header)
            {
                var length = BitConverter.ToInt32(_bufferLength, 0);

                if (length < 0)
                    throw new ProtocolViolationException("length cannot be less than zero");

                if (length > _maxBufferLength)
                    throw new ProtocolViolationException("the data length is greater than the maximum value.");

                _buffer = new byte[length];
                _receivingMode = ReceivingMode.Packet;
                _readingOffset += _bufferLength.Length;

                Read(data);
            }
            else
            {
                _receivedData += dataLength;

                if (_receivedData == _buffer.Length)
                {
                    DataReceivedEvent?.Invoke(_buffer);
                    if (CheckIfThereIsAnotherPacket(data))
                    {
                        Reset();
                        _readingOffset += dataLength;
                        Read(data);
                    }
                    else
                    {
                        Reset();
                    }
                }
                
                // we don't want to reset the offset until we read the rest of the data * another packet *
                _readingOffset = 0;

            }
        }

        private void Reset()
        {
            _buffer = null;
            _bufferLength = BitConverter.GetBytes(sizeof(int));
            _receivingMode = ReceivingMode.Header;
            _receivedData = 0;
        }

        private bool CheckIfThereIsAnotherPacket(byte[] data)
        {
            int dataLastIndex = data.Length - 1;
            int bufferLastIndex = _buffer.Length - 1;

            if (bufferLastIndex + _bufferLength.Length == dataLastIndex)
                return false;

            int checkLength = Math.Min(_buffer.Length, 4);

            // checks the last 4 bytes
            for (int i = 0; i < checkLength; i++)
                if (data[dataLastIndex - i] != _buffer[bufferLastIndex - i])
                    return true;

            return false;
        }

        public static byte[] Frame(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            var lengthInBytes = BitConverter.GetBytes(data.Length);
            var framedData = new byte[lengthInBytes.Length + data.Length];
            using (var frameDataMs = new MemoryStream(framedData))
            {
                using (var lengthMs = new MemoryStream(lengthInBytes))
                    lengthMs.WriteTo(frameDataMs);

                using (var dataMs = new MemoryStream(data))
                    dataMs.WriteTo(frameDataMs);
            }

            return framedData;
        }
    }
}