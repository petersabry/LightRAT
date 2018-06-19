using System;
using LightRAT.Network.Packets;

namespace LightRAT.Network
{
    public class ReceivePacketArgs : EventArgs
    {
        public IPacket Packet { get; }

        public ReceivePacketArgs(IPacket packet)
        {
            Packet = packet;
        }
    }
}
