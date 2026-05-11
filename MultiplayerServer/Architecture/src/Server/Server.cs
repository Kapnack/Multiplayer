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

        internal Server(string matchMakingIP, int portToConnect, int portToHost) : base()
        {
            InitEncryption();

            PacketTypeStrategy.Add(PacketType.Data, HandleData);

            matchMakerData = new MatchMakerData();

            Connect(portToHost);

            IPAddress ipAddress = IPAddress.Parse(matchMakingIP);
            matchMakerData.IPEndPoint = new IPEndPoint(ipAddress, portToConnect);

            Send(matchMakerData.IPEndPoint, PacketType.Handshake, BitConverter.GetBytes((byte)ConnectionRole.Server), PacketMetaData.Reliable);

            isConnectedToMatchMaking = true;
        }

        internal Server() : base()
        {
            InitEncryption();

            PacketTypeStrategy.Add(PacketType.Data, HandleData);

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

        private void SendPingToMatchMaker()
        {
            Send(matchMakerData.IPEndPoint, PacketType.Ping, BitConverter.GetBytes(DateTime.UtcNow.Ticks));
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

            long ticks = BitConverter.ToInt64(networkPacket.payload, 0);

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

            Send(ip, PacketType.Ping, BitConverter.GetBytes(DateTime.UtcNow.Ticks));
        }

        private void HandleData(NetworkPacket packet)
        {
            BroadcastRaw(packet.rawPacket, packet.ipEndPoint);
        }

        protected override void HandleHandShake(NetworkPacket networkPacket)
        {
            if (clients.ContainsKey(networkPacket.ipEndPoint))
            {
                Send(networkPacket.ipEndPoint, PacketType.Handshake, BitConverter.GetBytes(clients[networkPacket.ipEndPoint].id), PacketMetaData.Reliable);
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

            byte[] payload = new byte[sizeof(uint) * 2 + sizeof(int) + encryptorSeed.Length];

            BitConverter.GetBytes(0).CopyTo(payload, 0);
            BitConverter.GetBytes(newID).CopyTo(payload, sizeof(uint));
            BitConverter.GetBytes(encryptorSeed.Length).CopyTo(payload, sizeof(uint) * 2);
            encryptorSeed.CopyTo(payload, sizeof(uint) * 2 + sizeof(int));

            Send(networkPacket.ipEndPoint, PacketType.Handshake, payload, PacketMetaData.Reliable);

            foreach (KeyValuePair<IPEndPoint, ClientData> it in clients)
                if (!it.Key.Equals(networkPacket.ipEndPoint))
                    Send(networkPacket.ipEndPoint, PacketType.ClientJoined, BitConverter.GetBytes(it.Value.id), PacketMetaData.Reliable);

            Broadcast(networkPacket.ipEndPoint, PacketType.ClientJoined, BitConverter.GetBytes(newID), PacketMetaData.Reliable);
        }

        protected override void HandleClientLeft(NetworkPacket packet)
        {
            DisconectClient(packet.ipEndPoint);
        }

        void Broadcast(PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
        {
            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                Send(client.Key, type, payload, metaData);
            }
        }

        void Broadcast(IPEndPoint exception, PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
        {
            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                if (!client.Value.isConnected || client.Key.Equals(exception))
                    continue;

                Send(client.Key, type, payload, metaData);
            }
        }

        void BroadcastRaw(byte[] data, IPEndPoint exception)
        {
            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                if (client.Key.Equals(exception))
                    continue;

                SendRaw(data, client.Key);
            }
        }

        void DisconectClient(IPEndPoint ip)
        {
            if (!clients.ContainsKey(ip))
                return;

            Broadcast(ip, PacketType.ClientLeft, BitConverter.GetBytes(clients[ip].id), PacketMetaData.Reliable);

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