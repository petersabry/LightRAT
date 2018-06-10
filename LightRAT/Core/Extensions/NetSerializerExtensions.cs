using System;
using System.Collections.Generic;
using System.Linq;

namespace LightRAT.Core.Extensions
{
    public static class NetSerializerExtensions
    {
        public static IEnumerable<Type> GetSubTypes(this Type type)
        {
            return type.Assembly.GetTypes().Where(subtype => subtype.IsSubclassOf(type));
        }
    }
}