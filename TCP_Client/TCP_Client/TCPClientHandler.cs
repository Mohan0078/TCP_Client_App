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
        private static List<byte[]> _allPackets = new List<byte[]>(); // To keep track of received sequences

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

        /// <summary>
        /// Reads TCP server response and store the packets if any
        /// </summary>
        private static void ReadServerResponse()
        {
            byte[] buffer = new byte[17]; // Buffer to read each packet (total 17 bytes per packet)
            int bytesRead;

            while ((bytesRead = _networkStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Ensure we received a full packet (17 bytes)
                if (bytesRead == 0)
                {
                    break;
                }
                _allPackets.Add(buffer);
            }
        }
    }
}
