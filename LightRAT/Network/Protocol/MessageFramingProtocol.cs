using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;


namespace LightRAT.Network.Protocol
{
    public class MessageFramingProtocol
    {
        private readonly int _maxBufferLength;
        private byte[] _buffer;
        private byte[] _bufferLength = BitConverter.GetBytes(sizeof(int));
        private int _receivedData;
        private ReceivingMode _receivingMode = ReceivingMode.Header;
        private int _readingOffset;
        public event Func<byte[], Task> DataReceivedEvent;

        public MessageFramingProtocol(int maxBufferLength)
        {
            _maxBufferLength = maxBufferLength;
        }

        public void Read(byte[] data)
        {
            if (_receivingMode == ReceivingMode.Header) ReadHeader(data);
            if (_receivingMode == ReceivingMode.Packet) ReadIncomingData(data);
        }

        private void ReadHeader(byte[] data)
        {
                InternalRead(data, _bufferLength, _bufferLength.Length);

                var length = BitConverter.ToInt32(_bufferLength, 0);

                if (length < 0)
                    throw new ProtocolViolationException("length cannot be less than zero");

                if (length > _maxBufferLength)
                    throw new ProtocolViolationException("the data length is greater than the maximum value.");

                _buffer = new byte[length];
                _readingOffset += _bufferLength.Length;
                _receivingMode = ReceivingMode.Packet;
        }
        private void ReadIncomingData(byte[] data)
        {
            var remainingDataLength = _buffer.Length - _receivedData;

            // we need to know if the incoming data is smaller than the needed data or not, if so we will read it all.
            int length = Math.Min(remainingDataLength, data.Length);

            // this mainly used to skip the header
            if (length != remainingDataLength)
                length -= _readingOffset;

            InternalRead(data, _buffer, length);

            _receivedData += length;

            if (_receivedData == _buffer.Length)
            {
                DataReceivedEvent?.Invoke(_buffer);
                if (CheckIfThereIsAnotherPacket(data))
                {
                    Reset();
                    _readingOffset += length;
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

        private void InternalRead(byte[] data, byte[] readingBuffer, int length)
        {
            using (var bufferMemoryStream = new MemoryStream(readingBuffer, _receivedData, length))
                    using (var dataMemoryStream = new MemoryStream(data, _readingOffset, length, false))
                        dataMemoryStream.WriteTo(bufferMemoryStream);
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

        #region Asynchronous Methods
        public async Task ReadAsync(byte[] data)
        {
            if (_receivingMode == ReceivingMode.Header) await ReadHeaderAsync(data);
            if (_receivingMode == ReceivingMode.Packet) await ReadIncomingDataAsync(data);
        }

        private async Task ReadHeaderAsync(byte[] data)
        {
            await InternalReadAsync(data, _bufferLength, _bufferLength.Length);

            var length = BitConverter.ToInt32(_bufferLength, 0);

            if (length < 0)
                throw new ProtocolViolationException("length cannot be less than zero");

            if (length > _maxBufferLength)
                throw new ProtocolViolationException("the data length is greater than the maximum value.");

            _buffer = new byte[length];
            _readingOffset += _bufferLength.Length;
            _receivingMode = ReceivingMode.Packet;
        }

        private async Task ReadIncomingDataAsync(byte[] data)
        {
            var remainingDataLength = _buffer.Length - _receivedData;

            // we need to know if the incoming data is smaller than the needed data or not, if so we will read it all.
            int length = Math.Min(remainingDataLength, data.Length);

            // this mainly used to skip the header
            if (length != remainingDataLength)
                length -= _readingOffset;

            await InternalReadAsync(data, _buffer, length);

            _receivedData += length;

            if (_receivedData == _buffer.Length)
            {
                DataReceivedEvent?.Invoke(_buffer);
                if (CheckIfThereIsAnotherPacket(data))
                {
                    Reset();
                    _readingOffset += length;
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

        private async Task InternalReadAsync(byte[] data, byte[] readingBuffer, int length)
        {
            using (var bufferMemoryStream = new MemoryStream(readingBuffer, _receivedData, length))
            using (var dataMemoryStream = new MemoryStream(data, _readingOffset, length, false))
                await dataMemoryStream.CopyToAsync(bufferMemoryStream);
        }

        public static async Task<byte[]> FrameAsync(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("the data cannot be null", nameof(data));

            var lengthInBytes = BitConverter.GetBytes(data.Length);
            var framedData = new byte[lengthInBytes.Length + data.Length];

            using (var frameDataMs = new MemoryStream(framedData))
            {
                using (var lengthMs = new MemoryStream(lengthInBytes))
                    await lengthMs.CopyToAsync(frameDataMs);

                using (var dataMs = new MemoryStream(data))
                    await dataMs.CopyToAsync(frameDataMs);
            }

            return framedData;
        }
        #endregion
    }
}