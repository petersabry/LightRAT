using System;
using LightRAT.Core.Data;

namespace LightRAT.Core.Network.Packets
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