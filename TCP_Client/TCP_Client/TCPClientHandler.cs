using System.Net.Sockets;
using System.Text;

namespace TCP_Client
{
    public class TCPClientHandler
    {
        private const int Port = 3000;
        private const string ServerAddress = "127.0.0.1"; // Localhost
        private static TcpClient _tcpClient;
        private static NetworkStream _networkStream;

        public static void HandleTCPClient()
        {
            try
            {
                // Instantiate TCP Client
                InstantiateTCPClient();

                // Make get all packets request
                SendRequest(1, 0);

                // Get packets from server response
                var serverResponsePackets = ReadServerResponse();

                // Process server response
                var parsedServerResponse = ProcessServerResponse(serverResponsePackets);

                // Filter received sequences from parse server response
                var receivedSequences = parsedServerResponse.Select(x => x.Sequence).ToList();

                // Get missing packets
                var missingPackets = RequestMissingPackets(receivedSequences);

                if (missingPackets != null)
                    parsedServerResponse.AddRange(missingPackets);

                parsedServerResponse = parsedServerResponse.OrderBy(x => x.Sequence).ToList();

                foreach (var serverResponse in parsedServerResponse)
                {
                    Console.WriteLine($"Received Packet - Symbol: {serverResponse.Symbol}, " +
                        $"Buy/Sell: {Convert.ToChar(serverResponse.BuyOrSell)}, " +
                        $"Quantity: {serverResponse.Quantity}, " +
                        $"Price: {serverResponse.Price}, " +
                        $"Sequence: {serverResponse.Sequence}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong ! " + ex.Message);
            }
        }

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
        /// <returns>List of packets received from server</returns>
        private static List<byte[]> ReadServerResponse()
        {
            List<byte[]> serverResponse = new List<byte[]>();

            // Buffer to read each packet (total 17 bytes per packet)
            byte[] buffer = new byte[17];
            int bytesRead;

            while ((bytesRead = _networkStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                serverResponse.Add(buffer);
            }

            return serverResponse;
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

        /// <summary>
        /// Check for missing packet sequences and request them
        /// </summary>
        /// <param name="receivedSequences">All received sequences</param>
        /// <returns>Parsed missing packets list if found else returns null</returns>
        private static List<TCPServerPacketResponse> RequestMissingPackets(List<int> receivedSequences)
        {
            // for storing missing packets
            var missingPackets = new List<byte[]>();

            // Getting the maximum sequence number
            int highestReceivedSequence = receivedSequences.Max();

            InstantiateTCPClient();
            
            // Check for missing sequences (excluding the last one, which is never missing)
            for (int sequence = 1; sequence < highestReceivedSequence; sequence++)
            {
                if (!receivedSequences.Contains(sequence))
                {
                    // Resend Packet Request
                    SendRequest(2, (byte)sequence);
                    var serverResponse = ReadSingleServerResponse();
                    if(serverResponse != null)
                    missingPackets.Add(serverResponse);
                }
            }

            if(missingPackets.Count > 0)
            {
                var parsedServerResponse = ProcessServerResponse(missingPackets);
                return parsedServerResponse;
            }
            return null;
        }
        /// <summary>
        /// Read single server response
        /// </summary>
        /// <returns></returns>
        private static byte[] ReadSingleServerResponse()
        {
            byte[] buffer = new byte[17];
            int bytesRead;

            if ((bytesRead = _networkStream.Read(buffer, 0, buffer.Length)) > 0)
            {
               return buffer;
            }
            return null;
        }
    }
}
