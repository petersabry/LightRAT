namespace LightRAT.Network
{
    public struct NetworkSizes
    {
        public static int MaxPacketSize // 8mb
            => 8 * 1024 * 1024;

        public static int BufferSize //4kb
            => 4 * 1024;

        public static int HeaderSize // 4b
            => sizeof(int);
    }
}