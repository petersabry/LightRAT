using System.Net.Sockets;

namespace LightRAT.Core.Network
{
    public class Client
    {
        public Socket ClientSocket { get; }

        public Client(Socket socket)
        {
            ClientSocket = socket;
        }
    }
}
