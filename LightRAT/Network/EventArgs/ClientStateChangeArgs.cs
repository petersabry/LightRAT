using System;
using LightRAT.Network.Packets;

namespace LightRAT.Network
{
    public class ClientStateChangeArgs : EventArgs
    {
        public string Id { get; set; }
        public Client Client { get; set; }
        public IPacket InfoPacket { get; set; }

        public ClientStateChangeArgs(string id, Client client, IPacket infoPacket)
        {
            Id = id;
            Client = client;
            InfoPacket = infoPacket;
        }

        public ClientStateChangeArgs(string id, Client client)
        :this(id, client, null)
        {
            
        }
    }
}
