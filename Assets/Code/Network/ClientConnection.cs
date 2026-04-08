using Assets.Code.Architecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using KapNet;
using MultiplayerServer.src;
using Org.BouncyCastle.Asn1.Cms;
using ServerArquitecture.src.Server.Packets;
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

    private UdpConnection connection;
    private IPEndPoint serverEndPoint;

    private IClient client;

    MultiplayerServer.src.Time Time = new MultiplayerServer.src.Time();
    PacketFactory PacketFactory = new PacketFactory();

    private Dictionary<uint, float> recivedAndUsedPacket = new Dictionary<uint, float>();
    private List<PacketAwaitingResponce> packetsAwaitingResponce = new List<PacketAwaitingResponce>();
    private List<NetworkPacket> cryticalPackets = new List<NetworkPacket>();

    private readonly Dictionary<PacketType, PacketTypeDelegate> packetTypeStrategy;
    private readonly Dictionary<PacketMetaData, SendPacketMetaDataDelegate> sendingMetaDataStrategy;
    private readonly Dictionary<PacketMetaData, RecivePacketMetaDataDelegate> recivingMetaDataStrategy;

    private double lastServerResponce;

    public ClientConnection(IClient client)
    {
        this.client = client;

        packetTypeStrategy = new Dictionary<PacketType, PacketTypeDelegate>()
        {
            { PacketType.Pong, HandlePong },
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
            { PacketMetaData.Reliable, HandleReliablePacketRecived },
            { PacketMetaData.Crytical, HandleCriticalPacketRecived }
        };
    }

    public void Init()
    {
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
        connection = new UdpConnection(serverEndPoint.Address, serverEndPoint.Port, this);

        lastServerResponce = Time.RealTimeSinceStartUp;

        SendHandshake();
    }

    public void LateInit() { }

    public void Tick(float deltaTime)
    {
        Time.Tick();

        connection.FlushReceiveData();

        CheckPacketsToResent();
        CheckDiscartOfRecivedAndUsed();

        CheckServerIsResponding();

        SendPing();
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
        if (Time.RealTimeSinceStartUp - lastServerResponce > 10)
            OnServerShutDown();
    }

    private void OnServerShutDown()
    {
        Dispose();
        client.OnServerShutDown();
    }

    private void CheckPacketsToResent()
    {
        for (int i = 0; i < packetsAwaitingResponce.Count; i++)
        {
            var packet = packetsAwaitingResponce[i];

            if (Time.RealTimeSinceStartUp - packet.lastTimeSent > 3)
            {
                RawSend(packet.data);
                packet.lastTimeSent = Time.RealTimeSinceStartUp;
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
            Time.RealTimeSinceStartUp
        ));
    }

    private void HandleCriticalMessage(NetworkPacket packet, byte[] data)
    {
        cryticalPackets.Add(packet);
    }

    void Send(PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
    {
        (byte[] data, uint packetId) = PacketFactory.Create(type, payload, metaData);

        NetworkPacket networkPacket = new NetworkPacket(
            type,
            packetId,
            metaData,
            payload,
            (float)Time.RealTimeSinceStartUp
        );

        HandleSendMetaData(networkPacket, data);

        RawSend(data);
    }

    private void RawSend(byte[] data)
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
                strategy.Value(packet, data);
        }
    }

    private void HandleCriticalPacketRecived(NetworkPacket packet)
    {
        cryticalPackets.Add(packet);
    }

    private void HandleReliablePacketRecived(NetworkPacket packet)
    {
        Send(PacketType.Acknowledgement, BitConverter.GetBytes(packet.packetID));
        recivedAndUsedPacket[packet.packetID] = (float)Time.RealTimeSinceStartUp;
    }

    private void HandleClientJoined(NetworkPacket packet)
    {
        client.OnClienJoined(BitConverter.ToUInt32(packet.payload, 0));
    }

    void HandleData(NetworkPacket networkPacket)
    {
        client.OnPayloadRecieve(networkPacket.payload, (uint)networkPacket.clientId);
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
            (float)Time.RealTimeSinceStartUp,
            (int)userID,
            ipEndpoint
        );

        if (recivedAndUsedPacket.ContainsKey(packetID))
        {
            recivedAndUsedPacket[packetID] = (float)Time.RealTimeSinceStartUp;
            HandleAcknowledgement(networkPacket);
            return;
        }

        HandleRecivedMetaData(networkPacket);

        if (packetTypeStrategy.TryGetValue(type, out var handler))
            handler(networkPacket);
    }

    private void CheckDiscartOfRecivedAndUsed()
    {
        List<uint> toRemove = new List<uint>();

        foreach (var packet in recivedAndUsedPacket)
        {
            if (Time.RealTimeSinceStartUp - packet.Value > 10)
                toRemove.Add(packet.Key);
        }

        foreach (uint id in toRemove)
            recivedAndUsedPacket.Remove(id);
    }

    public void Send(byte[] payload, PacketMetaData metaData)
    {
        Send(PacketType.Data, payload, metaData);
    }

    public void Dispose()
    {
        connection.Close();
    }
}