using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace LightRAT.Core.Network
{
    public class Server : IDisposable
    {
        private object _clientStateChangedLock = new object();

        public IPEndPoint ServerEndPoint { get; set; }
        public Socket InternalSocket { get; } = new Socket(AddressFamily.InterNetwork ,SocketType.Stream, ProtocolType.Tcp);
        public List<Client> ConnectedClients { get; private set; } = new List<Client>();
        public bool IsDisposed { get; private set; } = false;


        public event Client.ReceiveDataEventHandler ReceiveDataEvent;
        public event Client.StateChangeEventHandler StateChangeEvent;

        public Server(string ip, int port)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }
        public void Start()
        {
            try
            {
                InternalSocket.Bind(ServerEndPoint);
                InternalSocket.Listen(500);
                InternalSocket.BeginAccept(EndAccepting, null);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                    throw new InvalidOperationException("The selected port is already used by another process");
                else
                    throw new NotSupportedException("oops unexpected error was thrown please report this issue to the developer.");
            }  
        }
        private void EndAccepting(IAsyncResult result)
        {
            var client = new Client(InternalSocket.EndAccept(result));
            AddClient(client);
            client.StartReceive();
            InternalSocket.BeginAccept(EndAccepting, null);
        }

        public void AddClient(Client client)
        {
            lock (_clientStateChangedLock)
            {
                client.ReceiveDataEvent += ReceiveDataEvent;
                client.StateChangeEvent += StateChangeEvent;
                ConnectedClients.Add(client);
            }
        }
        public void RemoveClient(Client client)
        {
            lock (_clientStateChangedLock)
            {
                client.ReceiveDataEvent -= ReceiveDataEvent;
                client.StateChangeEvent -= StateChangeEvent;
                ConnectedClients.Remove(client);
                client.Dispose();
            }
        }
        public void Dispose()
        {
            if (!IsDisposed)
            {
                ServerEndPoint = null;
                InternalSocket.Disconnect(false);
                InternalSocket.Shutdown(SocketShutdown.Both);
                InternalSocket.Dispose();
                foreach (var client in ConnectedClients)
                {
                    client.ReceiveDataEvent -= this.ReceiveDataEvent;
                    client.Dispose();
                }
                ConnectedClients = null;
                IsDisposed = true;
            }
        }
    }
}
