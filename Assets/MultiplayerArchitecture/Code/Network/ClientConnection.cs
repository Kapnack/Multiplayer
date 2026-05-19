using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Services;
using KapNet;
using KapNet.src;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using System;
using System.Net;

public class ClientConnection : NetworkPeer<uint>, IInitable, ITickable, IDisposable
{
    private IClient client;

    private DateTime lastServerResponce;

    private NetworkRegistry NetworkRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();

    public double Ping { get; private set; }

    public ClientConnection(IClient client) : base()
    {
        this.client = client;

        PacketTypeStrategy.Add(PacketType.ConnectToServer, HandleServerConnection);
        PacketTypeStrategy.Add(PacketType.Spawn, HandleEntitySpawn);
        PacketTypeStrategy.Add(PacketType.ClientJoined, HandleClientJoin);
        PacketTypeStrategy.Add(PacketType.Destroy, HandleDestroy);
        PacketTypeStrategy.Add(PacketType.Position, HandlePosition);
        PacketTypeStrategy.Add(PacketType.RejectedQueue, HandleRejectedQueue);
    }

    private void HandleRejectedQueue(NetworkPacket networkPacket)
    {
        client.OnRejectedQueue();
    }

    private void HandleClientJoin(NetworkPacket networkPacket)
    {
        foreach (Entity entity in NetworkRegistry.AllOfType<Entity>())
            Send(PacketType.Spawn, PacketMetaData.Reliable, entity.ownerNetworkID,
                entity.coordinate.x, entity.coordinate.y, entity.coordinate.z,
                entity.GetType().Name);
    }

    private void HandlePosition(NetworkPacket networkPacket)
    {
        uint clientID = packetReader.ReadUInt();
        uint entityID = packetReader.ReadUInt();
        Coordinate coordinate = new Coordinate(packetReader.ReadFloat(), packetReader.ReadFloat(), packetReader.ReadFloat());
        Rotation rotation = new Rotation(packetReader.ReadFloat(), packetReader.ReadFloat(), packetReader.ReadFloat(), packetReader.ReadFloat());

        client.OnPositionRecieve(clientID, entityID, coordinate, rotation);
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

        int port = packetReader.ReadInt();

        IPAddress ipAddress = new IPAddress(ipBytes);

        Connect(ipAddress, port);

        Send(PacketType.Handshake, PacketMetaData.Reliable);
    }

    protected override void HandleHandShake(NetworkPacket networkPacket)
    {
        uint id = packetReader.ReadUInt();
        NetworkID = id;
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
        Send(PacketType.Ping, PacketMetaData.None, DateTime.UtcNow.Ticks);
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

        SendPing();

        CheckServerIsResponding();
    }

    public void Dispose()
    {
        Disconnect();
    }

    protected override void HandlePing(NetworkPacket networkPacket)
    {
        long ticks = packetReader.ReadLong();

        DateTime sendTime = new DateTime(ticks, DateTimeKind.Utc);

        DateTime utcNow = DateTime.UtcNow;

        Ping = (utcNow - sendTime).TotalMilliseconds;

        lastServerResponce = utcNow;
    }

    protected override void HandleUnhandledPacket(NetworkPacket networkPacket)
    {
    }
}