namespace LightRAT.Network.Packets
{
    public interface IPacket
    {
        void Execute(Client client);
    }
}