using LightRAT.Core.Extensions;
using LightRAT.Core.Network.Packets;
using NetSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRAT.Core.Utils
{
    public static class LightRATUtils
    {
        private static IEnumerable<Type> subtypes = typeof(IPacket).GetSubTypes();
        public static Serializer packetSerializer = new Serializer(subtypes);
        
    }
}
