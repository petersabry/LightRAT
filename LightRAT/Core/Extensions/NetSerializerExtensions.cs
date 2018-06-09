using System;
using System.Collections.Generic;

namespace LightRAT.Core.Extensions
{
    public static class NetSerializerExtensions
    {
        public static IEnumerable<Type> GetSubTypes(this Type type)
        {
            foreach (var subtype in type.Assembly.GetTypes())
                if (subtype.IsSubclassOf(type))
                    yield return subtype;
        }
    }
}
