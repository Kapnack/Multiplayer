using System.Net;

namespace KapNet
{
    public class NetworkPacket
    {
        public byte[] data;
        public PacketType type;
        public uint packetID;
        public PacketMetaData metaData;
        public uint clientId;
        public byte[] payload;
        public IPEndPoint ipEndPoint;
        public float timeStamp;

        public NetworkPacket(byte[] data, PacketType type, uint packetID, PacketMetaData metaData, byte[] payload, float timeStamp, uint clientId = 0, IPEndPoint ipEndPoint = null)
        {
            this.data = data;
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