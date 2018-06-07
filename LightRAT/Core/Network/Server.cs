using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace LightRAT.Core.Network
{
    public class Server
    {
        public IPEndPoint ServerEndPoint { get; set; }
        public Socket InternalSocket { get; } = new Socket(SocketType.Stream, ProtocolType.Tcp);
        public List<Client> ConnectedClients { get; } = new List<Client>();

        public event Client.ReceiveDataEventHandler ReceiveEvent;

        public Server(string ip, int port)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void Start()
        {
            InternalSocket.Bind(ServerEndPoint);
            InternalSocket.Listen(10);
            InternalSocket.BeginAccept(EndAccepting, null);
        }
        private void EndAccepting(IAsyncResult result)
        {
            var client = new Client(InternalSocket.EndAccept(result));
            client.ReceiveDataEvent += ReceiveEvent;
            ConnectedClients.Add(client);
            InternalSocket.BeginAccept(EndAccepting, null);
        }
    }
}
