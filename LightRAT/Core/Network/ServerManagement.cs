using System;
using System.Collections.Generic;

namespace LightRAT.Core.Network
{
    public class ServerManagement
    {
        private readonly object _serverLock = new object();

        public List<Server> Servers { get; } = new List<Server>();

        public ServerManagement(Server server)
        {
            if(server == null)
                throw new ArgumentNullException("Server cannot be null", nameof(server));

            AddServer(server);
        }

        public void AddServer(Server server)
        {
            lock (_serverLock)
            {
                server.ClientReceiveDataEvent += Server_ClientReceiveDataEvent;
                server.ClientStateChangeEvent += Server_ClientStateChangeEvent;
                Servers.Add(server);
            }
        }
        public void RemoveServer(Server server)
        {
            lock (_serverLock)
            {
                server.ClientReceiveDataEvent -= Server_ClientReceiveDataEvent;
                server.ClientStateChangeEvent -= Server_ClientStateChangeEvent;
                Servers.Remove(server);
                server.Dispose();
            }
        }

        private void Server_ClientStateChangeEvent(Server server, Client client, ClientState state)
        {
            throw new NotImplementedException();
        }

        private void Server_ClientReceiveDataEvent(Server server, Client client, Packets.IPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}
