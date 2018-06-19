using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetSerializer;

namespace LightRAT
{
    public static class NetSerializerExtensions
    {
        public static IEnumerable<Type> GetSubTypes(this Type type) =>
            type.Assembly.GetTypes().Where(subtype => subtype.GetInterfaces().Contains(type));

        public static Task SerializeAsync(this Serializer serializer, Stream stream, object obj) =>
            Task.Run(() => serializer.Serialize(stream, obj));

        public static Task<object> DeserializeAsync(this Serializer serializer, Stream stream) =>
            Task.Run(() => serializer.Deserialize(stream));
    }
}