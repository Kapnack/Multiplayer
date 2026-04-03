using KapNet;
using System.Net;


public class NetworkPacket
{
    public PacketType type;
    public uint packetID;
    public PacketMetaData metaData;
    public int clientId;
    public byte[] payload;
    public IPEndPoint ipEndPoint;
    public float timeStamp;

    public NetworkPacket(PacketType type, uint packetID, PacketMetaData metaData, byte[] payload, float timeStamp, int clientId = -1, IPEndPoint ipEndPoint = null)
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