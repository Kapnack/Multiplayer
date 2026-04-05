using ImageCampus.ToolBox.Services;
using KapNet;
using ServerArquitecture.src.Server.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

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

public class MyClient : MonoBehaviour, IReceiveData
{
    private delegate void PacketTypeDelegate(NetworkPacket networkPacket);
    private delegate void SendPacketMetaDataDelegate(NetworkPacket packet, byte[] data);
    private delegate void RecivePacketMetaDataDelegate(NetworkPacket packet);

    UdpConnection client;
    IPEndPoint serverEndPoint;

    public GameObject playerPrefab;

    uint myID;

    Dictionary<uint, GameObject> players = new Dictionary<uint, GameObject>();

    PacketFactory PacketFactory => ServiceProvider.Instance.GetService<PacketFactory>();

    private Dictionary<uint, float> recivedAndUsedPacket = new Dictionary<uint, float>();
    private List<PacketAwaitingResponce> packetsAwaitingResponce = new List<PacketAwaitingResponce>();
    private List<NetworkPacket> cryticalPackets = new List<NetworkPacket>();

    private readonly Dictionary<PacketType, PacketTypeDelegate> packetTypeStrategy;
    private readonly Dictionary<PacketMetaData, SendPacketMetaDataDelegate> sendingMetaDataStrategy;
    private readonly Dictionary<PacketMetaData, RecivePacketMetaDataDelegate> recivingMetaDataStrategy;

    public MyClient()
    {
        packetTypeStrategy = new Dictionary<PacketType, PacketTypeDelegate>()
        {
            { PacketType.Acknowledgement, HandleAcknowledgement },
            { PacketType.Data, HandleData },
            { PacketType.ClientLeft, HandleClientLeft },
            { PacketType.ClientJoined, HandleSpawn },
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

    private void HandleID(NetworkPacket networkPacket)
    {
        uint userID = BitConverter.ToUInt32(networkPacket.payload, 0);

        myID = userID;

        Debug.Log("My ID: " + myID);

        SpawnPlayer(myID, Vector3.one * 3);
    }

    void Start()
    {
        Application.runInBackground = true;

        Screen.fullScreen = false;
        Screen.SetResolution(800, 600, false);

        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
        client = new UdpConnection(serverEndPoint.Address, serverEndPoint.Port, this);

        ServiceProvider.Instance.AddService<PacketFactory>(new PacketFactory());

        SendHandshake();
    }

    void Update()
    {
        client.FlushReceiveData();

        CheckPacketsToResent();
        CheckDiscartOfRecivedAndUsed();

        if (myID == 0)
            return;

        SendPing();

        if (players.ContainsKey(myID))
            SendPosition(players[myID].transform.position);

    }

    private void CheckPacketsToResent()
    {
        for (int i = 0; i < packetsAwaitingResponce.Count; i++)
        {
            PacketAwaitingResponce packet = packetsAwaitingResponce[i];

            if (Time.realtimeSinceStartup - packet.lastTimeSent > 3)
            {
                client.Send(packet.data, packet.ipEndPoint);

                packet.lastTimeSent = Time.realtimeSinceStartup;
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
        uint playerID = BitConverter.ToUInt32(packet.payload);
        Destroy(players[playerID]);
        players.Remove(playerID);
    }

    private void HandleReliableMessageSend(NetworkPacket packet, byte[] data)
    {
        packetsAwaitingResponce.Add(new PacketAwaitingResponce(
            packet.packetID,
            data,
            packet.ipEndPoint,
            Time.realtimeSinceStartup
        ));
    }

    private void HandleCriticalMessage(NetworkPacket packet, byte[] data)
    {
        cryticalPackets.Add(packet);
    }

    void SendPosition(Vector3 pos)
    {
        byte[] payload = new byte[12];

        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, payload, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, payload, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, payload, 8, 4);

        Send(PacketType.Data, payload);
    }

    void Send(PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
    {
        (byte[] data, uint packetId) = PacketFactory.Create(type, payload, metaData);

        NetworkPacket networkPacket = new NetworkPacket(
            type,
            packetId,
            metaData,
            payload,
            Time.realtimeSinceStartup,
            (int)myID,
            null
        );

        if (type != PacketType.Ping && type != PacketType.Data)
            Debug.Log($"[SENDING PACKET] {{{Time.realtimeSinceStartup}}} Type:{networkPacket.type}, PacketID: {networkPacket.packetID}, User: {networkPacket.clientId} {networkPacket.ipEndPoint}");

        HandleSendMetaData(networkPacket, data);

        SendRaw(data);
    }

    void SendRaw(byte[] data)
    {
        client.Send(data);
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
        recivedAndUsedPacket[packet.packetID] = Time.realtimeSinceStartup;
    }

    void HandleSpawn(NetworkPacket packet)
    {
        uint newUserID = BitConverter.ToUInt32(packet.payload, 0);

        Debug.Log("Spawn player: " + newUserID);

        SpawnPlayer(newUserID, Vector3.one * 3);
    }

    void HandleData(NetworkPacket networkPacket)
    {
        uint id = BitConverter.ToUInt32(networkPacket.payload, 0);

        if (id == myID)
            return;

        float x = BitConverter.ToSingle(networkPacket.payload, 4);
        float y = BitConverter.ToSingle(networkPacket.payload, 8);
        float z = BitConverter.ToSingle(networkPacket.payload, 12);

        if (!players.ContainsKey(id))
            return;

        players[id].transform.position = new Vector3(x, y, z);
    }

    private void HandleAcknowledgement(NetworkPacket networkPacket)
    {
        uint packetID = BitConverter.ToUInt32(networkPacket.payload, 0);
        packetsAwaitingResponce.RemoveAll(p => p.packetID == packetID);
    }

    void SpawnPlayer(uint id, Vector3 pos)
    {
        GameObject obj = Instantiate(playerPrefab, pos, Quaternion.identity);

        players[id] = obj;

        if (id == myID)
        {
            obj.GetComponent<Renderer>().material.color = Color.green;
            obj.AddComponent<PlayerController>();
        }
        else
        {
            obj.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    void OnApplicationQuit()
    {
        Send(PacketType.ClientLeft);

        client.Close();
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
            Time.realtimeSinceStartup,
            (int)userID,
            ipEndpoint
        );

        if (type != PacketType.Pong && type != PacketType.Data)
            Debug.Log($"[RECIVED PACKET] {{{Time.realtimeSinceStartup}}} Type: {networkPacket.type}, PacketID: {networkPacket.packetID}, User: {networkPacket.clientId} {networkPacket.ipEndPoint}");

        if (recivedAndUsedPacket.ContainsKey(packetID))
        {
            recivedAndUsedPacket[packetID] = (float)Time.realtimeSinceStartup;
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
            if (Time.realtimeSinceStartup - networkPacket.Value > 10)
                toRemove.Add(networkPacket.Key);
        }

        foreach (uint packetID in toRemove)
            recivedAndUsedPacket.Remove(packetID);
    }

}