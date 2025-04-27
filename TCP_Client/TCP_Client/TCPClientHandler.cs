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

        /// <summary>
        /// Parse response packet
        /// </summary>
        /// <param name="buffer">Buffer packet to parse</param>
        /// <returns>Returns a parsed object</returns>
        private static TCPServerPacketResponse ParseResponsePacket(byte[] buffer)
        {
            // Parse the response packet fields
            string symbol = Encoding.ASCII.GetString(buffer, 0, 4);  // 4 bytes for the symbol
            byte buySellIndicator = buffer[4];                        // 1 byte for the Buy/Sell Indicator
            int quantity = BitConverter.ToInt32(buffer, 5);           // 4 bytes for Quantity (Big Endian)
            int price = BitConverter.ToInt32(buffer, 9);              // 4 bytes for Price (Big Endian)
            int sequence = BitConverter.ToInt32(buffer, 13);          // 4 bytes for Packet Sequence (Big Endian)

            quantity = BitConverter.ToInt32(BitConverter.GetBytes(quantity).Reverse().ToArray(), 0);
            price = BitConverter.ToInt32(BitConverter.GetBytes(price).Reverse().ToArray(), 0);
            sequence = BitConverter.ToInt32(BitConverter.GetBytes(sequence).Reverse().ToArray(), 0);

            return new TCPServerPacketResponse()
            {
                Symbol = symbol,
                BuyOrSell = Convert.ToChar(buySellIndicator),
                Quantity = quantity,
                Price = price,
                Sequence = sequence
            };
        }

        /// <summary>
        /// Process server response
        /// </summary>
        /// <param name="responsePackets">Response packets</param>
        /// <returns>Parsed response packets</returns>
        private static List<TCPServerPacketResponse> ProcessServerResponse(List<byte[]> responsePackets)
        {
            var tcpServerResponseList = new List<TCPServerPacketResponse>();

            foreach (var responsePacket in responsePackets)
            {
                // parse each packet
                var tcpServerResponse = ParseResponsePacket(responsePacket);
                // add parsed result to final list
                tcpServerResponseList.Add(tcpServerResponse);
            }
            return tcpServerResponseList;
        }
    }
}
