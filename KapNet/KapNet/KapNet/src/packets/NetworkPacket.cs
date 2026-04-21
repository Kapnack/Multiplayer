using System.Net;

namespace KapNet
{
    public class NetworkPacket
    {
        public PacketType type;
        public uint packetID;
        public PacketMetaData metaData;
        public byte[] payload;
        public IPEndPoint ipEndPoint;
        public long timeStamp;

        public NetworkPacket(PacketType type, uint packetID, PacketMetaData metaData, byte[] payload, long timeStamp, IPEndPoint ipEndPoint = null)
        {
            this.type = type;
            this.packetID = packetID;
            this.metaData = metaData;
            this.timeStamp = timeStamp;
            this.clientId = clientId;
            this.ipEndPoint = ipEndPoint;
            this.payload = payload;
        }
    }
}