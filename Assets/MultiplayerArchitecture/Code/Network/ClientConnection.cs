using Assets.MultiplayerArchitecture.Code.Network;
using Assets.MultiplayerArchitecture.Code.Network.packets;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Services;
using MultiplayerServer.src;
using System;
using System.Collections.Generic;
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

public class ClientConnection : IReceiveData, IInitable, ITickable, IDisposable
{
    private delegate void PacketTypeDelegate(NetworkPacket networkPacket);
    private delegate void SendPacketMetaDataDelegate(NetworkPacket packet, byte[] data);
    private delegate void RecivePacketMetaDataDelegate(NetworkPacket packet);

    UdpConnection connection;
    IPEndPoint serverEndPoint;

    private IClient client;

    PacketFactory packetFactory = new PacketFactory();
    Time time = new Time();

    private Dictionary<uint, float> recivedAndUsedPacket = new Dictionary<uint, float>();
    private List<PacketAwaitingResponce> packetsAwaitingResponce = new List<PacketAwaitingResponce>();
    private List<NetworkPacket> cryticalPackets = new List<NetworkPacket>();

    private readonly Dictionary<PacketType, PacketTypeDelegate> packetTypeStrategy;
    private readonly Dictionary<PacketMetaData, SendPacketMetaDataDelegate> sendingMetaDataStrategy;
    private readonly Dictionary<PacketMetaData, RecivePacketMetaDataDelegate> recivingMetaDataStrategy;

    private float lastServerResponce;

    public ClientConnection(IClient client)
    {
        this.client = client;

        packetTypeStrategy = new Dictionary<PacketType, PacketTypeDelegate>()
        {
            { PacketType.Ping, HandlePong },
            { PacketType.Acknowledgement, HandleAcknowledgement },
            { PacketType.Data, HandleData },
            { PacketType.ClientLeft, HandleClientLeft },
            { PacketType.ClientJoined, HandleClientJoined },
            { PacketType.SendID, HandleID },
        };

        sendingMetaDataStrategy = new Dictionary<PacketMetaData, SendPacketMetaDataDelegate>()
        {
            { PacketMetaData.Reliable, HandleReliableMessageSend },
            { PacketMetaData.Crytical, HandleCriticalMessage }
        };

        recivingMetaDataStrategy = new Dictionary<PacketMetaData, RecivePacketMetaDataDelegate>()
        {
            {PacketMetaData.Reliable, HandleReliablePacketRecived },
            {PacketMetaData.Crytical, HandleCriticalPacketRecived }
        };
    }

    private void HandlePong(NetworkPacket networkPacket)
    {
        lastServerResponce = networkPacket.timeStamp;
    }

    private void HandleID(NetworkPacket networkPacket)
    {
        client.OnHandShake(BitConverter.ToUInt32(networkPacket.payload, 0));
    }

    private void CheckServerIsResponding()
    {
        if (time.RealTimeSinceStartUp - lastServerResponce > 10)
            client.OnServerShutDown();
    }

    private void CheckPacketsToResent()
    {
        for (int i = 0; i < packetsAwaitingResponce.Count; i++)
        {
            PacketAwaitingResponce packet = packetsAwaitingResponce[i];

            if (time.RealTimeSinceStartUp - packet.lastTimeSent > 3)
            {
                connection.Send(packet.data, packet.ipEndPoint);

                packet.lastTimeSent = time.RealTimeSinceStartUp;
            }
        }
    }

    void SendHandshake()
    {
        Send(PacketType.Handshake, null, PacketMetaData.Reliable);
    }

    void SendPing()
    {
        Send(PacketType.Ping);
    }

    private void HandleClientLeft(NetworkPacket packet)
    {
        client.OnClientLeft(BitConverter.ToUInt32(packet.payload));
    }

    private void HandleReliableMessageSend(NetworkPacket packet, byte[] data)
    {
        packetsAwaitingResponce.Add(new PacketAwaitingResponce(
            packet.packetID,
            data,
            packet.ipEndPoint,
            time.RealTimeSinceStartUp
        ));
    }

    private void HandleCriticalMessage(NetworkPacket packet, byte[] data)
    {
        cryticalPackets.Add(packet);
    }

    void Send(PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
    {
        (byte[] data, uint packetId) = packetFactory.Create(type, payload, metaData);

        NetworkPacket networkPacket = new NetworkPacket(
            type,
            packetId,
            metaData,
            payload,
            time.realtimeSinceStartup
        );

        HandleSendMetaData(networkPacket, data);

        SendRaw(data);
    }

    public void Send(byte[] payload, PacketMetaData metaData = PacketMetaData.None)
    {
        Send(PacketType.Data, payload, metaData);
    }

    void SendRaw(byte[] data)
    {
        connection.Send(data);
    }

    private void HandleRecivedMetaData(NetworkPacket packet)
    {
        foreach (KeyValuePair<PacketMetaData, RecivePacketMetaDataDelegate> strategy in recivingMetaDataStrategy)
        {
            if (packet.metaData.HasFlag(strategy.Key))
                strategy.Value(packet);
        }
    }

    private void HandleSendMetaData(NetworkPacket packet, byte[] data)
    {
        foreach (KeyValuePair<PacketMetaData, SendPacketMetaDataDelegate> strategy in sendingMetaDataStrategy)
        {
            if (packet.metaData.HasFlag(strategy.Key))
            {
                strategy.Value(packet, data);
            }
        }
    }

    private void HandleCriticalPacketRecived(NetworkPacket packet)
    {
        cryticalPackets.Add(packet);
    }

    private void HandleReliablePacketRecived(NetworkPacket packet)
    {
        Send(PacketType.Acknowledgement, BitConverter.GetBytes(packet.packetID));
        recivedAndUsedPacket[packet.packetID] = (float)time.RealTimeSinceStartUp;
    }

    void HandleClientJoined(NetworkPacket packet)
    {
        client.OnClienJoined(BitConverter.ToUInt32(packet.payload, 0));
    }

    void HandleData(NetworkPacket networkPacket)
    {
        uint clientID = BitConverter.ToUInt32(networkPacket.payload, 0);

        byte[] newPayload = new byte[networkPacket.payload.Length - sizeof(uint)];

        Buffer.BlockCopy(networkPacket.payload, sizeof(uint), newPayload, 0, newPayload.Length);

        client.OnPayloadRecieve(newPayload, clientID);
    }

    private void HandleAcknowledgement(NetworkPacket networkPacket)
    {
        uint packetID = BitConverter.ToUInt32(networkPacket.payload, 0);
        packetsAwaitingResponce.RemoveAll(p => p.packetID == packetID);
    }

    public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
    {
        PacketType type = PacketUtility.GetType(data);
        uint packetID = PacketUtility.GetPacketID(data);
        PacketMetaData metaData = PacketUtility.GetMetaData(data);
        byte[] payload = PacketUtility.GetPayload(data);
        uint userID = PacketUtility.GetClientID(data);

        NetworkPacket networkPacket = new NetworkPacket(
            type,
            packetID,
            metaData,
            payload,
            (float)time.RealTimeSinceStartUp,
            (int)userID,
            ipEndpoint
        );

        if (recivedAndUsedPacket.ContainsKey(packetID))
        {
            recivedAndUsedPacket[packetID] = (float)time.RealTimeSinceStartUp;
            HandleAcknowledgement(networkPacket);
            return;
        }

        HandleRecivedMetaData(networkPacket);

        if (packetTypeStrategy.TryGetValue(type, out PacketTypeDelegate handler))
            handler(networkPacket);
    }

    void CheckDiscartOfRecivedAndUsed()
    {
        List<uint> toRemove = new List<uint>();

        foreach (KeyValuePair<uint, float> networkPacket in recivedAndUsedPacket)
        {
            if (time.realtimeSinceStartup - networkPacket.Value > 10)
                toRemove.Add(networkPacket.Key);
        }

        foreach (uint packetID in toRemove)
            recivedAndUsedPacket.Remove(packetID);
    }

    public void Init()
    {
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
        connection = new UdpConnection(serverEndPoint.Address, serverEndPoint.Port, this);

        ServiceProvider.Instance.AddService<PacketFactory>(new PacketFactory());

        lastServerResponce = time.realtimeSinceStartup;

        SendHandshake();
    }

    public void LateInit()
    {
    }

    public void Tick(float deltaTime)
    {
        connection.FlushReceiveData();

        CheckPacketsToResent();
        CheckDiscartOfRecivedAndUsed();

        CheckServerIsResponding();

        SendPing();
    }

    public void Dispose()
    {
        Send(PacketType.ClientLeft);

        connection.Close();
    }
}