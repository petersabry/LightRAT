namespace LightRAT.Core.Network
{
    public struct NetworkSizes
    {
        public static int MaxPacketSize { get { return 8 * 1024 * 1024; } } // 8mb
        public static int BufferSize { get { return 8 * 1024; } } //8kb
        public static int HeaderSize { get { return sizeof(int); } } // 4b
    }
}
