namespace TCP_Client
{
    public class TCPServerPacketResponse
    {
        public string Symbol { get; set; }
        public char BuyOrSell { get; set; }
        public Int32 Quantity { get; set; }
        public Int32 Price { get; set; }
        public Int32 Sequence { get; set; }
    }
}
