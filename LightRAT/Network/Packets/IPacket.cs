using System.Threading.Tasks;

namespace LightRAT.Network.Packets
{
    public interface IPacket
    {
        Task Execute(Client client);
    }
}