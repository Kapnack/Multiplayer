using System.Net;

public class PacketAwaitingResponce
{
    public uint packetID;
    public byte[] data;
    public IPEndPoint ipEndPoint;
    public double lastTimeSent;

    public PacketAwaitingResponce(uint packetID, byte[] data, IPEndPoint ipEndPoint, double lastTimeSent)
    {
        this.packetID = packetID;
        this.data = data;
        this.ipEndPoint = ipEndPoint;
        this.lastTimeSent = lastTimeSent;
    }
}
