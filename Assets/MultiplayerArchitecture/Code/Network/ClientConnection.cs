using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using KapNet;
using KapNet.src;
using MultiplayerArchitecture;
using System;
using System.Net;

public class ClientConnection : NetworkPeer<uint>, IInitable, ITickable, IDisposable
{
    private IClient client;

    private DateTime lastServerResponce;

    public double Ping { get; private set; }

    public ClientConnection(IClient client) : base()
    {
        this.client = client;

        PacketTypeStrategy.Add(PacketType.ConnectToServer, HandleServerConnection);
        PacketTypeStrategy.Add(PacketType.Spawn, HandleEntitySpawn);
        PacketTypeStrategy.Add(PacketType.Destroy, HandleDestroy);
        PacketTypeStrategy.Add(PacketType.Position, HandlePosition);
    }

    private void HandlePosition(NetworkPacket networkPacket)
    {
        uint clientID = packetReader.ReadUInt();
        uint entityID = packetReader.ReadUInt();
        Coordinate coordinate = new Coordinate(packetReader.ReadFloat(), packetReader.ReadFloat(), packetReader.ReadFloat());

        client.OnPositionRecieve(clientID, entityID, coordinate);
    }

    private void HandleDestroy(NetworkPacket networkPacket)
    {
        uint clientID = packetReader.ReadUInt();
        uint entityID = packetReader.ReadUInt();

        client.OnDestroyEntity(clientID, entityID);
    }

    private void HandleEntitySpawn(NetworkPacket networkPacket)
    {
        uint clientID = packetReader.ReadUInt();
        uint entityID = packetReader.ReadUInt();
        Coordinate coordinate = new Coordinate(packetReader.ReadFloat(), packetReader.ReadFloat(), packetReader.ReadFloat());
        string entityToSpawn = packetReader.ReadString();

        client.OnSpawn(clientID, entityID, coordinate, entityToSpawn);
    }

    private void HandleServerConnection(NetworkPacket networkPacket)
    {
        byte[] ipBytes = packetReader.ReadBytes();

        byte[] portBytes = packetReader.ReadBytes();

        IPAddress ipAddress = new IPAddress(ipBytes);
        int port = BitConverter.ToInt32(portBytes, 0);

        Send(PacketType.ClientLeft);

        Disconnect();

        Connect(ipAddress, port);

        Send(PacketType.Handshake);
    }

    protected override void HandleHandShake(NetworkPacket networkPacket)
    {
        NetworkID = packetReader.ReadUInt();
        byte[] encryptionSeed = packetReader.ReadBytes();

        packetEncryptor = new PacketEncryptor(encryptionSeed);
        client.OnHandShake(NetworkID);
    }

    private void CheckServerIsResponding()
    {
        if ((DateTime.UtcNow - lastServerResponce).TotalSeconds > 10)
            client.OnServerShutDown();
    }

    void SendPing()
    {
        Send(PacketType.Ping, PacketMetaData.None, BitConverter.GetBytes(DateTime.UtcNow.Ticks));
    }

    protected override void HandleClientLeft(NetworkPacket packet)
    {
        client.OnClientLeft(BitConverter.ToUInt32(packet.payload));
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

        if (NetworkID.Equals(0))
            return;

        CheckServerIsResponding();

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

    protected override void HandleUnhandledPacket(IPEndPoint packet, byte[] data)
    {
    }
}