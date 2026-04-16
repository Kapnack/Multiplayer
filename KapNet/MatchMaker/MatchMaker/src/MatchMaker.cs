using KapNet.src;
using ServerArquitecture.src;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal class MatchMaker : NetworkPeer<IPEndPoint>, IReceiveData
    {
        private const int port = 7777;
        private const int timeout = 10;

        private Dictionary<IPEndPoint, double> clients = new Dictionary<IPEndPoint, double>();
        private Queue<IPEndPoint> clientQueue = new Queue<IPEndPoint>();
        private List<ServerInfo> servers = new List<ServerInfo>();

        private int lasOpenPort = port;

        public MatchMaker() : base()
        {
            Connect(port);
        }

        public void Init()
        {
        }

        public void LateInit()
        {
            ServerConsole.Log("Server started");
        }

        public override void Tick()
        {
            base.Tick();

            CheckUserTimeouts();
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

        protected override void HandlePing(NetworkPacket packet)
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

        protected override void HandleHandShake(NetworkPacket networkPacket)
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

                for (int i = 0; i < servers.Count; ++i)
                {
                    ServerInfo info = servers[i];

                    if (info.clients.Count < info.maxPlayers)
                    {
                        IPEndPoint clientIP = networkPacket.ipEndPoint;

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

                clients[networkPacket.ipEndPoint] = Time.RealTimeSinceStartUp;

                clientQueue.Enqueue(networkPacket.ipEndPoint);

                if (clientQueue.Count < 2)
                    return;

                if (servers.Count == 0)
                    CreateServer();
                else if (AreServersFull())
                    CreateServer();
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

        protected override void HandleClientLeft(NetworkPacket packet)
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
    }
}
