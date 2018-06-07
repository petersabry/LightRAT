using LightRAT.Core.Network.Engine;
using LightRAT.Core.Network.Protocol;
using System;
using System.Net;
using System.Net.Sockets;

namespace LightRAT.Core.Network
{
    public class Client : IDisposable
    {
        public Socket ClientSocket { get; }

        public delegate void ReceiveDataEventHandler(string Data);
        public event ReceiveDataEventHandler ReceiveDataEvent;
        
        private MessageFramingProtocol protocol = new MessageFramingProtocol(NetworkSizes.MaxPacketSize);
        private byte[] _receivingBuffer = new byte[NetworkSizes.BufferSize];
        
        public Client(Socket socket)
        {
            ClientSocket = socket;
            protocol.DataReceivedEvent += MessageFraming_DataReceivedEvent;
            ClientSocket.BeginReceive(_receivingBuffer, 0, _receivingBuffer.Length, SocketFlags.None, ReceiveData, null);
        }

        private void MessageFraming_DataReceivedEvent(byte[] obj)
        {
            var decompressed = CryptEngine.Decompress(obj);
            ReceiveDataEvent(decompressed);
        }

        private void ReceiveData(IAsyncResult result)
        {
            try
            {
                ClientSocket.EndReceive(result);
                protocol.Read(_receivingBuffer);
                _receivingBuffer = new byte[NetworkSizes.BufferSize];
                ClientSocket.BeginReceive(_receivingBuffer, 0, _receivingBuffer.Length, SocketFlags.None, ReceiveData, null);
            }
            catch (SocketException)
            {
                throw;
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
        }

        public void Dispose()
        {
            _receivingBuffer = null;
            protocol.DataReceivedEvent -= MessageFraming_DataReceivedEvent;
            protocol = null;
            ClientSocket.Close();
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Dispose();
        }
    }
}
