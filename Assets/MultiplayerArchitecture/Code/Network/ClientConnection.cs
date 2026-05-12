using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using KapNet;
using KapNet.src;
using System;
using System.Net;

public class ClientConnection : NetworkPeer<uint>, IInitable, ITickable, IDisposable
{
    private IClient client;

    private DateTime lastServerResponce;

    private uint MyID = 0;

    public double Ping { get; private set; }

    public ClientConnection(IClient client) : base()
    {
        this.client = client;

        PacketTypeStrategy.Add(PacketType.ConnectToServer, HandleServerConnection);
        PacketTypeStrategy.Add(PacketType.Data, HandleData);
        PacketTypeStrategy.Add(PacketType.ClientJoined, HandleClientJoined);
    }

    private void HandleServerConnection(NetworkPacket networkPacket)
    {
        byte[] payload = networkPacket.payload;

        byte[] ipBytes = new byte[4];
        Buffer.BlockCopy(payload, 0, ipBytes, 0, 4);

        byte[] portBytes = new byte[4];
        Buffer.BlockCopy(payload, 4, portBytes, 0, 4);

        IPAddress ipAddress = new IPAddress(ipBytes);
        int port = BitConverter.ToInt32(portBytes, 0);

        Send(PacketType.ClientLeft);

        Disconnect();

        Connect(ipAddress, port);

        SendHandshake(new byte[0]);
    }

    protected override void HandleHandShake(NetworkPacket networkPacket)
    {
        MyID = BitConverter.ToUInt32(networkPacket.payload, sizeof(uint));
        int seedBytes = BitConverter.ToInt32(networkPacket.payload, sizeof(uint) * 2);

        byte[] encryptionSeed = new byte[seedBytes];

        Buffer.BlockCopy(encryptionSeed, 0, networkPacket.payload, sizeof(uint) * 2 + sizeof(int), seedBytes);

        packetEncryptor = new PacketEncryptor(encryptionSeed);
        client.OnHandShake(MyID);
    }

    private void CheckServerIsResponding()
    {
        if ((DateTime.UtcNow - lastServerResponce).TotalSeconds > 10)
            client.OnServerShutDown();
    }

    public void SendHandshake(byte[] payload)
    {
        byte[] newPayload = new byte[sizeof(uint) + sizeof(ConnectionRole) + payload.Length];

        BitConverter.GetBytes(MyID).CopyTo(newPayload, 0);
        newPayload[sizeof(uint)] = (byte)ConnectionRole.Client;
        Buffer.BlockCopy(payload, 0, newPayload, sizeof(uint) + sizeof(ConnectionRole), payload.Length);

        Send(PacketType.Handshake, newPayload, PacketMetaData.Reliable);
    }

    public void SendPacket(PacketType type, byte[] payload, PacketMetaData metaData)
    {
        Send(type, payload, metaData);
    }

    void SendPing()
    {
        Send(PacketType.Ping, BitConverter.GetBytes(DateTime.UtcNow.Ticks));
    }

    protected override void HandleClientLeft(NetworkPacket packet)
    {
        client.OnClientLeft(BitConverter.ToUInt32(packet.payload));
    }

    private void HandleClientJoined(NetworkPacket packet)
    {
        client.OnClienJoined(BitConverter.ToUInt32(packet.payload, 0));
    }

    void HandleData(NetworkPacket networkPacket)
    {
        uint clientID = BitConverter.ToUInt32(networkPacket.payload, 0);

        if (clientID == MyID)
            return;

        byte[] newPayload = new byte[networkPacket.payload.Length - sizeof(uint)];

        Buffer.BlockCopy(networkPacket.payload, sizeof(uint), newPayload, 0, newPayload.Length);

        client.OnPayloadRecieve(newPayload, clientID);
    }

    public void Init()
    {
        lastServerResponce = DateTime.UtcNow;
    }

    public void LateInit()
    {
    }

    public void Tick(float deltaTime)
    {
        Tick();

        CheckServerIsResponding();

        if (MyID.Equals(0))
            return;

        SendPing();
    }

    public void Dispose()
    {
        Send(PacketType.ClientLeft);

        Disconnect();
    }

    protected override void HandlePing(NetworkPacket networkPacket)
    {
        long ticks = BitConverter.ToInt64(networkPacket.payload, 0);

        DateTime sendTime = new DateTime(ticks, DateTimeKind.Utc);

        DateTime utcNow = DateTime.UtcNow;

        Ping = (utcNow - sendTime).TotalMilliseconds;

        lastServerResponce = utcNow;
    }
}