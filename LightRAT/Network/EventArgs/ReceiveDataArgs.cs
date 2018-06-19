using LightRAT.Network.Packets;

namespace LightRAT.Network.EventArgs
{
    public class ReceivePacketArgs : System.EventArgs
    {
        public IPacket Packet { get; }

        public ReceivePacketArgs(IPacket packet)
        {
            Packet = packet;
        }
    }
}
