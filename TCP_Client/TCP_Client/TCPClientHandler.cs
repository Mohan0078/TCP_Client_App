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

        /// <summary>
        /// Instantiate TCP Client
        /// </summary>
        private static void InstantiateTCPClient()
        {
            // Connect to the server
            _tcpClient = new TcpClient(ServerAddress, Port);
            _networkStream = _tcpClient.GetStream();
        }

        /// <summary>
        /// Function to send a request with a specific callType and resendSeq
        /// </summary>
        /// <param name="callType">Call type (1 for stream all, 2 for resend packet)</param>
        /// <param name="resendSeq">Resend sequence number (only used for call type 2)</param>
        private static void SendRequest(byte callType, byte resendSeq)
        {
            byte[] requestPayload = new byte[2];
            requestPayload[0] = callType; 
            requestPayload[1] = resendSeq;

            // Send the request
            _networkStream.Write(requestPayload, 0, requestPayload.Length);
            Console.WriteLine($"Sent request: CallType={callType}, ResendSeq={resendSeq}");
        }
    }
}
