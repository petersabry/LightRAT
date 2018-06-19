using System;
using LightRAT.Data;

namespace LightRAT.Network.Packets
{
    [Serializable]
    public class AuthenticationPacket : IPacket
    {
        public Account Account { get; set; }

        public void Execute(Client client)
        {
            client.SendPacket(this);
        }
    }
}