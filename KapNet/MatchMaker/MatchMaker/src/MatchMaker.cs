using ImageCampus.ToolBox.Services;
using KapNet;
using MultiplayerServer.src;
using ServerArquitecture.src;
using ServerArquitecture.src.Server.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

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

class ServerInfo
{
    public List<IPEndPoint> clients;
    public IPEndPoint endPoint;
    public int port;
    public int maxPlayers;
    public double lastHeartbeat;
}

namespace KapNet
{
    internal class MatchMaker : IReceiveData
    {
        private delegate void PacketTypeDelegate(NetworkPacket networkPacket);
        private delegate void SendPacketMetaDataDelegate(NetworkPacket packet, byte[] data);
        private delegate void RecivePacketMetaDataDelegate(NetworkPacket packet);

       private const int port = 7777;
       private const int timeout = 10;

        private UdpConnection connection;
        Time Time = ServiceProvider.Instance.GetService<Time>();
        PacketFactory PacketFactory => ServiceProvider.Instance.GetService<PacketFactory>();

        private RSACryptoServiceProvider rsa;
        private string publicKey;

        private Dictionary<IPEndPoint, double> clients = new Dictionary<IPEndPoint, double>();
        private Queue<IPEndPoint> clientQueue = new Queue<IPEndPoint>();
        private List<ServerInfo> servers = new List<ServerInfo>();

        private Dictionary<IPEndPoint, Dictionary<uint, double>> recivedAndUsedPacket = new Dictionary<IPEndPoint, Dictionary<uint, double>>();
        private List<PacketAwaitingResponce> packetsAwaitingResponce = new List<PacketAwaitingResponce>();
        private List<NetworkPacket> cryticalPackets = new List<NetworkPacket>();

        private readonly Dictionary<PacketType, PacketTypeDelegate> packetTypeStrategy;
        private readonly Dictionary<PacketMetaData, SendPacketMetaDataDelegate> sendingMetaDataStrategy;
        private readonly Dictionary<PacketMetaData, RecivePacketMetaDataDelegate> recivingMetaDataStrategy;

        private int lasOpenPort = port;

        public MatchMaker()
        {
            connection = new UdpConnection(port, this);

            packetTypeStrategy = new Dictionary<PacketType, PacketTypeDelegate>()
            {
                { PacketType.Handshake, HandleHandShake },
                { PacketType.Ping, HandlePing },
                { PacketType.ClientLeft, HandleClientLeft },
                { PacketType.Acknowledgement, HandleAcknowledgement }
            };

            sendingMetaDataStrategy = new Dictionary<PacketMetaData, SendPacketMetaDataDelegate>()
            {
                { PacketMetaData.Reliable, HandleReliableMessageSend },
                { PacketMetaData.Crytical, HandleCriticalMessage }
            };

            recivingMetaDataStrategy = new Dictionary<PacketMetaData, RecivePacketMetaDataDelegate>()
            {
                {PacketMetaData.Reliable, HandleReliablePacketRecived }
            };
        }

        private void HandleAcknowledgement(NetworkPacket networkPacket)
        {
            uint packetID = BitConverter.ToUInt32(networkPacket.payload, 0);
            packetsAwaitingResponce.RemoveAll(p => p.packetID == packetID);
        }

        private void HandleReliablePacketRecived(NetworkPacket packet)
        {
            Send(packet.ipEndPoint, PacketType.Acknowledgement, BitConverter.GetBytes(packet.packetID));

            if (!recivedAndUsedPacket.ContainsKey(packet.ipEndPoint))
                recivedAndUsedPacket[packet.ipEndPoint] = new Dictionary<uint, double>();

            recivedAndUsedPacket[packet.ipEndPoint][packet.packetID] = (float)Time.RealTimeSinceStartUp;
        }

        public void Init()
        {
            rsa = new RSACryptoServiceProvider();
            publicKey = rsa.ToXmlString(false);
        }

        public void LateInit()
        {
            ServerConsole.Log("Server started");
        }

        public void Tick()
        {
            connection?.FlushReceiveData();
            CheckUserTimeouts();
            CheckPacketsToResent();
            CheckDiscartOfRecivedAndUsed();
            CheckServersTimeouts();
        }

        void Unload()
        {
            foreach (KeyValuePair<IPEndPoint, double> client in clients)
                DisconnectClient(client.Key);
        }

        public void Dispose()
        {
            ServerConsole.Log("Shutting down..");
            Unload();
            ServerConsole.Log("Server Closed");
        }

        public void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            PacketType type = PacketUtility.GetType(data);
            uint packetID = PacketUtility.GetPacketID(data);
            PacketMetaData metaData = PacketUtility.GetMetaData(data);
            byte[] payload = PacketUtility.GetPayload(data);

            NetworkPacket networkPacket = new NetworkPacket(
                type,
                packetID,
                metaData,
                payload,
                (float)Time.RealTimeSinceStartUp,
                -1,
                ip
            );

            if (type != PacketType.Ping && type != PacketType.Data)
                ServerConsole.Log($"[RECEIVED PACKET] {{{Time.RealTimeSinceStartUp}}} Type:\x1B[33m {networkPacket.type}\u001b[0m, PacketID: {networkPacket.packetID}, User: {networkPacket.ipEndPoint}");

            if (recivedAndUsedPacket.TryGetValue(ip, out Dictionary<uint, double> packets))
            {
                if (packets.TryGetValue(packetID, out double timeStamp))
                {
                    timeStamp = (float)Time.RealTimeSinceStartUp;
                    HandleAcknowledgement(networkPacket);
                    return;
                }
            }

            HandleRecivedMetaData(networkPacket);

            if (packetTypeStrategy.TryGetValue(networkPacket.type, out PacketTypeDelegate handler))
                handler(networkPacket);
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

        private void HandlePing(NetworkPacket packet)
        {
            IPEndPoint ip = packet.ipEndPoint;

            if (clients.ContainsKey(ip))
                clients[ip] = Time.RealTimeSinceStartUp;

            foreach (ServerInfo server in servers)
            {
                if (server.endPoint.Equals(ip))
                {
                    server.lastHeartbeat = Time.RealTimeSinceStartUp;
                    break;
                }
            }

            Send(ip, PacketType.Ping);
        }

        private void HandleHandShake(NetworkPacket networkPacket)
        {
            if (clients.ContainsKey(networkPacket.ipEndPoint))
                return;

            if ((ConnectionRole)networkPacket.payload[0] == ConnectionRole.Client)
            {
                ServerInfo server = ShouldUserBeInMatch(networkPacket.ipEndPoint);

                if (server != null)
                {
                    byte[] ipBytes = server.endPoint.Address.GetAddressBytes();
                    byte[] portBytes = BitConverter.GetBytes(server.port);

                    byte[] payload = new byte[ipBytes.Length + portBytes.Length];
                    Buffer.BlockCopy(ipBytes, 0, payload, 0, ipBytes.Length);
                    Buffer.BlockCopy(portBytes, 0, payload, ipBytes.Length, portBytes.Length);

                    Send(networkPacket.ipEndPoint, PacketType.ConnectToServer, payload, PacketMetaData.Reliable);

                    return;
                }

                clients[networkPacket.ipEndPoint] = Time.RealTimeSinceStartUp;

                clientQueue.Enqueue(networkPacket.ipEndPoint);

                if (clientQueue.Count < 2)
                    return;

                if (servers.Count == 0)
                    CreateServer();
                else if (AreServersFull())
                    CreateServer();
                else
                    for (int i = 0; i < servers.Count; ++i)
                    {
                        ServerInfo info = servers[i];

                        if (info.clients.Count < info.maxPlayers)
                        {
                            IPEndPoint clientIP = clientQueue.Dequeue();

                            byte[] ipBytes = info.endPoint.Address.GetAddressBytes();
                            byte[] portBytes = BitConverter.GetBytes(info.port);

                            byte[] payload = new byte[ipBytes.Length + portBytes.Length];
                            Buffer.BlockCopy(ipBytes, 0, payload, 0, ipBytes.Length);
                            Buffer.BlockCopy(portBytes, 0, payload, ipBytes.Length, portBytes.Length);

                            info.clients.Add(clientIP);

                            Send(clientIP, PacketType.ConnectToServer, payload, PacketMetaData.Reliable);
                            break;
                        }
                    }

            }
            else
            {
                ServerInfo newServer = new ServerInfo()
                {
                    clients = new List<IPEndPoint>(),
                    endPoint = networkPacket.ipEndPoint,
                    port = lasOpenPort,
                    maxPlayers = 5,
                    lastHeartbeat = Time.RealTimeSinceStartUp
                };

                servers.Add(newServer);

                byte[] ipBytes = newServer.endPoint.Address.GetAddressBytes();
                byte[] portBytes = BitConverter.GetBytes(newServer.port);

                byte[] payload = new byte[ipBytes.Length + portBytes.Length];
                Buffer.BlockCopy(ipBytes, 0, payload, 0, ipBytes.Length);
                Buffer.BlockCopy(portBytes, 0, payload, ipBytes.Length, portBytes.Length);

                for (int i = 0; i < newServer.maxPlayers; ++i)
                {
                    if (clientQueue.Count == 0)
                        return;

                    IPEndPoint client = clientQueue.Dequeue();

                    clients.Remove(client);

                    Send(client, PacketType.ConnectToServer, payload, PacketMetaData.Reliable);

                    newServer.clients.Add(client);
                }
            }
        }

        private ServerInfo ShouldUserBeInMatch(IPEndPoint joiningClient)
        {
            foreach (ServerInfo serverInfo in servers)
                foreach (IPEndPoint client in serverInfo.clients)
                    if (client.Equals(joiningClient))
                        return serverInfo;

            return null;
        }

        private bool AreServersFull()
        {
            foreach (ServerInfo server in servers)
            {
                if (server.clients.Count < server.maxPlayers)
                    return false;
            }

            return true;
        }

        private void CreateServer()
        {
            ++lasOpenPort;
            Process.Start("View.exe", $"127.0.0.1 7777 {lasOpenPort}");
        }

        private void HandleClientLeft(NetworkPacket packet)
        {
            DisconnectClient(packet.ipEndPoint);
        }

        private void RemoveClientFromQueue(IPEndPoint clientToRemove)
        {
            Queue<IPEndPoint> newQueue = new Queue<IPEndPoint>();

            while (clientQueue.Count > 0)
            {
                IPEndPoint client = clientQueue.Dequeue();

                if (!client.Equals(clientToRemove))
                    newQueue.Enqueue(client);
            }

            clientQueue = newQueue;
        }

        private void DisconnectClient(IPEndPoint ipEndPoint)
        {
            clients.Remove(ipEndPoint);

            RemoveClientFromQueue(ipEndPoint);
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

        void Send(IPEndPoint ip, PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
        {
            (byte[] data, uint packetId) = PacketFactory.Create(type, payload, metaData);

            NetworkPacket networkPacket = new NetworkPacket(
                type,
                packetId,
                metaData,
                payload,
                (float)Time.RealTimeSinceStartUp,
                -1,
                ip
            );

            if (type != PacketType.Ping && type != PacketType.Data)
                ServerConsole.Log($"[SENDING PACKET] {{{Time.RealTimeSinceStartUp}}} Type:\x1B[33m {networkPacket.type}\u001b[0m, PacketID: {networkPacket.packetID}, User: {networkPacket.clientId} {networkPacket.ipEndPoint}");

            HandleSendMetaData(networkPacket, data);

            SendRaw(data, ip);
        }

        void SendRaw(byte[] data, IPEndPoint ip)
        {
            connection.Send(data, ip);
        }

        void CheckUserTimeouts()
        {
            List<IPEndPoint> toRemove = new List<IPEndPoint>();

            foreach (KeyValuePair<IPEndPoint, double> client in clients)
            {
                if (Time.RealTimeSinceStartUp - client.Value > timeout)
                    toRemove.Add(client.Key);
            }

            foreach (IPEndPoint ip in toRemove)
            {
                ServerConsole.Log("Client timeout: " + ip);
                DisconnectClient(ip);
            }
        }

        private void CheckServersTimeouts()
        {
            List<ServerInfo> toRemove = new List<ServerInfo>();

            foreach (ServerInfo server in servers)
            {
                if (Time.RealTimeSinceStartUp - server.lastHeartbeat > timeout)
                    toRemove.Add(server);
            }

            foreach (ServerInfo server in toRemove)
                servers.Remove(server);
        }

        void CheckPacketsToResent()
        {
            for (int i = 0; i < packetsAwaitingResponce.Count; i++)
            {
                PacketAwaitingResponce packet = packetsAwaitingResponce[i];

                if (Time.RealTimeSinceStartUp - packet.lastTimeSent > 3)
                {
                    SendRaw(packet.data, packet.ipEndPoint);

                    packet.lastTimeSent = Time.RealTimeSinceStartUp;
                }
            }
        }

        void CheckDiscartOfRecivedAndUsed()
        {
            List<(IPEndPoint user, uint packetId)> toRemove = new List<(IPEndPoint user, uint packetId)>();

            foreach (KeyValuePair<IPEndPoint, Dictionary<uint, double>> userEntry in recivedAndUsedPacket)
            {
                foreach (KeyValuePair<uint, double> packetEntry in userEntry.Value)
                {
                    if (Time.RealTimeSinceStartUp - packetEntry.Value > 10)
                    {
                        toRemove.Add((userEntry.Key, packetEntry.Key));
                    }
                }
            }

            foreach ((IPEndPoint user, uint packetId) entry in toRemove)
            {
                if (recivedAndUsedPacket.TryGetValue(entry.user, out Dictionary<uint, double> packets))
                {
                    packets.Remove(entry.packetId);

                    if (packets.Count == 0)
                        recivedAndUsedPacket.Remove(entry.user);
                }
            }
        }
    }
}
