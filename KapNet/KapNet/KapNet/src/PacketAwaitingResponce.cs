using System.Net;

public class PacketAwaitingResponce
{
    public byte[] data;
    public IPEndPoint ipEndPoint;
    public double lastTimeSent;

    public PacketAwaitingResponce(byte[] data, IPEndPoint ipEndPoint, double lastTimeSent)
    {
        this.data = data;
        this.ipEndPoint = ipEndPoint;
        this.lastTimeSent = lastTimeSent;
    }
}
