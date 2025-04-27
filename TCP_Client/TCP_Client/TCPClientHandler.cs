using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCP_Client
{
    public class TCPClientHandler
    {
        private const int Port = 3000;
        private const string ServerAddress = "127.0.0.1"; // Localhost
        private static TcpClient _tcpClient;
        private static NetworkStream _networkStream;

        private static void InstantiateTCPClient()
        {
            // Connect to the server
            _tcpClient = new TcpClient(ServerAddress, Port);
            _networkStream = _tcpClient.GetStream();
        }
    }
}
