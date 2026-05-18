using ImageCampus.ToolBox.Dataflow;
using KapNet.src;
using ServerArquitecture.src;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;

namespace KapNet
{
    public class ClientData
    {
        public string userName;
        public uint id;
        public bool isConnected;
        public DateTime lastResponce;
        public double ping;
    }

    public struct MatchMakerData
    {
        public IPEndPoint IPEndPoint;
        public DateTime lastResponce;
        public double ping;
    }

    public class Server : NetworkPeer<IPEndPoint>, IInitable, ITickable, IDisposable
    {
        public int port = 7777;
        public int timeout = 10;

        MatchMakerData matchMakerData;

        private Dictionary<IPEndPoint, ClientData> clients = new Dictionary<IPEndPoint, ClientData>();

        private uint currentClientID = 0;

        private bool isConnectedToMatchMaking = false;

        private byte[] encryptorSeed;
        uint levelID = 0;

        internal Server(string matchMakingIP, int portToConnect, int portToHost, uint levelID) : base()
        {
            InitEncryption();

            this.levelID = levelID;

            matchMakerData = new MatchMakerData();

            Connect(portToHost);

            IPAddress ipAddress = IPAddress.Parse(matchMakingIP);
            matchMakerData.IPEndPoint = new IPEndPoint(ipAddress, portToConnect);

            Send(matchMakerData.IPEndPoint, PacketType.Handshake, PacketMetaData.Reliable, (byte)ConnectionRole.Server, levelID);

            isConnectedToMatchMaking = true;
        }

        internal Server() : base()
        {
            InitEncryption();

            Connect(7777);
        }

        private void InitEncryption()
        {
            encryptorSeed = new byte[4];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(encryptorSeed);
            }

            packetEncryptor = new PacketEncryptor(encryptorSeed);
        }

        public void Init()
        {
        }

        public void LateInit()
        {
            ServerConsole.Log("Server started");
        }

        public void Tick(float deltaTime)
        {
            base.Tick();

            CheckUserTimeouts();

            if (!isConnectedToMatchMaking)
                return;

            SendPingToMatchMaker();

            if ((DateTime.UtcNow - matchMakerData.lastResponce).TotalSeconds > timeout)
                isConnectedToMatchMaking = false;
        }

        protected override void HandleUnhandledPacket(NetworkPacket networkPacket)
        {
            Broadcast(networkPacket);
        }

        private void SendPingToMatchMaker()
        {
            Send(matchMakerData.IPEndPoint, PacketType.Ping, PacketMetaData.None, DateTime.UtcNow.Ticks);
        }

        void Unload()
        {
            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                DisconectClient(client.Key);
            }
        }

        public void Dispose()
        {
            ServerConsole.Log("Shutting down..");
            Unload();
            ServerConsole.Log("Server Closed");
        }

        protected override void HandlePing(NetworkPacket networkPacket)
        {
            IPEndPoint ip = networkPacket.ipEndPoint;

            long ticks = packetReader.ReadLong();

            DateTime sendTime = new DateTime(ticks, DateTimeKind.Utc);

            DateTime utcNow = DateTime.UtcNow;

            double ping = (utcNow - sendTime).TotalMilliseconds;

            if (ip.Equals(matchMakerData.IPEndPoint))
            {
                matchMakerData.lastResponce = DateTime.UtcNow;
                matchMakerData.ping = ping;
            }
            else if (clients.ContainsKey(ip))
            {
                clients[ip].lastResponce = DateTime.UtcNow;
                clients[ip].ping = ping;
            }

            Send(ip, PacketType.Ping, PacketMetaData.None, DateTime.UtcNow.Ticks);
        }

        protected override void HandleHandShake(NetworkPacket networkPacket)
        {
            if (clients.ContainsKey(networkPacket.ipEndPoint))
            {
                Send(networkPacket.ipEndPoint, PacketType.Handshake, PacketMetaData.Reliable, clients[networkPacket.ipEndPoint].id, encryptorSeed);
                clients[networkPacket.ipEndPoint].isConnected = true;
                clients[networkPacket.ipEndPoint].lastResponce = DateTime.UtcNow;
                return;
            }

            ++currentClientID;
            uint newID = currentClientID;

            lock (clients)
                clients.Add(networkPacket.ipEndPoint, new ClientData
                {
                    id = newID,
                    isConnected = true,
                    lastResponce = DateTime.UtcNow
                });

            ServerConsole.Log($"Client connected: {networkPacket.ipEndPoint} ID: {newID}");

            Send(networkPacket.ipEndPoint, PacketType.Handshake, PacketMetaData.Reliable, newID, encryptorSeed);

            foreach (KeyValuePair<IPEndPoint, ClientData> it in clients)
                if (!it.Key.Equals(networkPacket.ipEndPoint))
                    Send(networkPacket.ipEndPoint, PacketType.ClientJoined, PacketMetaData.Reliable, it.Value.id);

            Broadcast(networkPacket.ipEndPoint, PacketType.ClientJoined, PacketMetaData.Reliable, newID);
        }

        protected override void HandleClientLeft(NetworkPacket packet)
        {
            DisconectClient(packet.ipEndPoint);
        }

        void Broadcast(IPEndPoint expection, PacketType type, PacketMetaData metaData = PacketMetaData.None, params object[] parameters)
        {
            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                if (!client.Value.isConnected || client.Key.Equals(expection))
                    continue;

                Send(client.Key, type, metaData, parameters);
            }
        }

        void Broadcast(NetworkPacket networkPacket)
        {
            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                if (!client.Value.isConnected || client.Key.Equals(networkPacket.ipEndPoint))
                    continue;

                SendByteArrayRaw(networkPacket.type, networkPacket.metaData, networkPacket.payload);
            }
        }

        void DisconectClient(IPEndPoint ip)
        {
            if (!clients.ContainsKey(ip))
                return;

            Broadcast(ip, PacketType.ClientLeft, PacketMetaData.Reliable, clients[ip].id);

            clients[ip].isConnected = false;

            ServerConsole.Log("Client removed: " + ip);
        }

        void CheckUserTimeouts()
        {
            List<IPEndPoint> toRemove = new List<IPEndPoint>();

            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                if (client.Value.isConnected)
                    if ((DateTime.UtcNow - client.Value.lastResponce).TotalSeconds > timeout)
                        toRemove.Add(client.Key);
            }

            foreach (IPEndPoint ip in toRemove)
            {
                ServerConsole.Log("Client timeout: " + ip);
                DisconectClient(ip);
            }
        }
    }
}