using System.Net.Sockets;

namespace LightRDP.Core.Network
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
