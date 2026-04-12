using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Services;
using MultiplayerServer.src;
using ServerArquitecture.src;
using ServerArquitecture.src.Server.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;

namespace KapNet
{
    public class ClientData
    {
        public uint id;
        public double lastResponce;
    }

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

    public class Server : IReceiveData, IInitable, ITickable, IDisposable
    {
        private delegate void PacketTypeDelegate(NetworkPacket networkPacket);
        private delegate void SendPacketMetaDataDelegate(NetworkPacket packet, byte[] data);
        private delegate void RecivePacketMetaDataDelegate(NetworkPacket packet);

        public int port = 7777;
        public int timeout = 10;

        private UdpConnection connection;
        Time Time => ServiceProvider.Instance.GetService<Time>();
        PacketFactory PacketFactory = new PacketFactory();

        IPEndPoint matchMakingIP;
        private double matchMakerLastResponce;

        private RSACryptoServiceProvider rsa;
        private string publicKey;

        private Dictionary<IPEndPoint, ClientData> clients = new Dictionary<IPEndPoint, ClientData>();

        private Dictionary<IPEndPoint, Dictionary<uint, float>> recivedAndUsedPacket = new Dictionary<IPEndPoint, Dictionary<uint, float>>();
        private List<PacketAwaitingResponce> packetsAwaitingResponce = new List<PacketAwaitingResponce>();
        private List<NetworkPacket> cryticalPackets = new List<NetworkPacket>();
        private Dictionary<IPEndPoint, SortedDictionary<uint, NetworkPacket>> orderedPackets = new Dictionary<IPEndPoint, SortedDictionary<uint, NetworkPacket>>();
        private Dictionary<IPEndPoint, uint> nextExpectedPacket = new Dictionary<IPEndPoint, uint>();

        private readonly Dictionary<PacketType, PacketTypeDelegate> packetTypeStrategy;
        private readonly Dictionary<PacketMetaData, SendPacketMetaDataDelegate> sendingMetaDataStrategy;
        private readonly Dictionary<PacketMetaData, RecivePacketMetaDataDelegate> recivingMetaDataStrategy;

        private uint currentClientID = 0;

        private bool isConnectedToMatchMaking = false;

        internal Server(string matchMakingIP, int portToConnect, int portToHost)
        {
            if (portToHost < 0 || matchMakingIP == "" || portToConnect < 0)
                Environment.Exit(0);

            packetTypeStrategy = new Dictionary<PacketType, PacketTypeDelegate>()
            {
                { PacketType.Handshake, HandleHandShake },
                { PacketType.Ping, HandlePing },
                { PacketType.Data, HandleData },
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
                {PacketMetaData.Reliable, HandleReliablePacketRecived },
                {PacketMetaData.Crytical, HandleOrdenable },
                {PacketMetaData.Ordenable, HandleOrdenable }
            };

            connection = new UdpConnection(portToHost, this);

            IPAddress ipAddress = IPAddress.Parse(matchMakingIP);
            this.matchMakingIP = new IPEndPoint(ipAddress, portToConnect);

            connection.Connect(ipAddress, portToConnect);

            Send(PacketType.Handshake, BitConverter.GetBytes((byte)ConnectionRole.Server), PacketMetaData.Reliable);

            isConnectedToMatchMaking = true;
        }

        internal Server()
        {
            connection = new UdpConnection(port, this);

            packetTypeStrategy = new Dictionary<PacketType, PacketTypeDelegate>()
            {
                { PacketType.Handshake, HandleHandShake },
                { PacketType.Ping, HandlePing },
                { PacketType.Data, HandleData },
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
                {PacketMetaData.Reliable, HandleReliablePacketRecived },
                {PacketMetaData.Crytical, HandleOrdenable },
                {PacketMetaData.Ordenable, HandleOrdenable }
            };
        }

        private void HandleOrdenable(NetworkPacket packet)
        {
            if (!orderedPackets.ContainsKey(packet.ipEndPoint))
            {
                orderedPackets[packet.ipEndPoint] = new SortedDictionary<uint, NetworkPacket>();
                nextExpectedPacket[packet.ipEndPoint] = 0;
            }

            orderedPackets[packet.ipEndPoint][packet.packetID] = packet;

            ProcessOrderedPackets(packet.ipEndPoint);
        }

        private void ProcessOrderedPackets(IPEndPoint ip)
        {
            SortedDictionary<uint, NetworkPacket> buffer = orderedPackets[ip];

            while (buffer.TryGetValue(nextExpectedPacket[ip], out NetworkPacket packet))
            {
                buffer.Remove(nextExpectedPacket[ip]);

                if (packetTypeStrategy.TryGetValue(packet.type, out PacketTypeDelegate handler))
                {
                    handler(packet);
                }

                nextExpectedPacket[ip]++;
            }
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
                recivedAndUsedPacket[packet.ipEndPoint] = new Dictionary<uint, float>();

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

        public void Tick(float deltaTime)
        {
            connection?.FlushReceiveData();
            CheckUserTimeouts();
            CheckPacketsToResent();
            CheckDiscartOfRecivedAndUsed();

            if (!isConnectedToMatchMaking)
                return;

            SendPingToMatchMaker();

            if (Time.RealTimeSinceStartUp - matchMakerLastResponce > timeout)
                isConnectedToMatchMaking = false;
        }

        private void SendPingToMatchMaker()
        {
            (byte[] data, uint packetID) = PacketFactory.Create(PacketType.Ping);

            connection.Send(data);
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
                clients.ContainsKey(ip) ? (int)clients[ip].id : -1,
                ip
            );

            if (type != PacketType.Ping && type != PacketType.Data)
                ServerConsole.Log($"[RECEIVED PACKET] {{{Time.RealTimeSinceStartUp}}} Type:\x1B[33m {networkPacket.type}\u001b[0m, PacketID: {networkPacket.packetID}, User: {networkPacket.clientId} {networkPacket.ipEndPoint}");

            lock (recivedAndUsedPacket)
            {
                if (recivedAndUsedPacket.TryGetValue(ip, out Dictionary<uint, float> packets))
                {
                    if (packets.TryGetValue(packetID, out float timeStamp))
                    {
                        timeStamp = (float)Time.RealTimeSinceStartUp;
                        HandleAcknowledgement(networkPacket);
                        return;
                    }
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
            if (packet.ipEndPoint.Equals(matchMakingIP))
                matchMakerLastResponce = Time.RealTimeSinceStartUp;

            IPEndPoint ip = packet.ipEndPoint;

            if (clients.ContainsKey(ip))
                clients[ip].lastResponce = Time.RealTimeSinceStartUp;

            Send(ip, PacketType.Ping);
        }

        private void HandleData(NetworkPacket packet)
        {
            byte[] newPayload = PacketUtility.Combine(
                BitConverter.GetBytes(packet.clientId),
                packet.payload
            );

            Broadcast(packet.ipEndPoint, PacketType.Data, newPayload, packet.metaData);
        }

        private void HandleHandShake(NetworkPacket networkPacket)
        {
            if (clients.ContainsKey(networkPacket.ipEndPoint))
                return;

            ++currentClientID;
            uint newID = currentClientID;

            lock (clients)
                clients.Add(networkPacket.ipEndPoint, new ClientData
                {
                    id = newID,
                    lastResponce = Time.RealTimeSinceStartUp
                });

            ServerConsole.Log($"Client connected: {networkPacket.ipEndPoint} ID: {newID}");

            Send(networkPacket.ipEndPoint, PacketType.SendID, BitConverter.GetBytes(newID), PacketMetaData.Reliable);

            foreach (KeyValuePair<IPEndPoint, ClientData> it in clients)
                if (!it.Key.Equals(networkPacket.ipEndPoint))
                    Send(networkPacket.ipEndPoint, PacketType.ClientJoined, BitConverter.GetBytes(it.Value.id), PacketMetaData.Reliable);

            Broadcast(networkPacket.ipEndPoint, PacketType.ClientJoined, BitConverter.GetBytes(newID), PacketMetaData.Reliable);
        }

        private void HandleClientLeft(NetworkPacket packet)
        {
            DisconectClient(packet.ipEndPoint);
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
                clients.ContainsKey(ip) ? (int)clients[ip].id : -1,
                ip
            );

            if (type != PacketType.Ping && type != PacketType.Data)
                ServerConsole.Log($"[SENDING PACKET] {{{Time.RealTimeSinceStartUp}}} Type:\x1B[33m {networkPacket.type}\u001b[0m, PacketID: {networkPacket.packetID}, User: {networkPacket.clientId} {networkPacket.ipEndPoint}");

            HandleSendMetaData(networkPacket, data);

            SendRaw(data, ip);
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

            if (type != PacketType.Ping && type != PacketType.Data)
                ServerConsole.Log($"[SENDING PACKET] {{{Time.RealTimeSinceStartUp}}} Type:\x1B[33m {networkPacket.type}\u001b[0m, PacketID: {networkPacket.packetID}, User: {networkPacket.clientId} {networkPacket.ipEndPoint}");

            HandleSendMetaData(networkPacket, data);

            SendRaw(data);
        }

        void SendRaw(byte[] data, IPEndPoint ip = null)
        {
            if (ip == null)
                connection.Send(data);
            else
            {
                if (ip.Equals(matchMakingIP))
                    connection.Send(data);
                else
                    connection.Send(data, ip);
            }
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
                if (client.Key.Equals(exception))
                    continue;

                Send(client.Key, type, payload, metaData);
            }
        }

        void DisconectClient(IPEndPoint ip)
        {
            if (!clients.ContainsKey(ip))
                return;

            Broadcast(ip, PacketType.ClientLeft, BitConverter.GetBytes(clients[ip].id), PacketMetaData.Reliable);

            clients.Remove(ip);
            ServerConsole.Log("Client removed: " + ip);
        }

        void CheckUserTimeouts()
        {
            List<IPEndPoint> toRemove = new List<IPEndPoint>();

            foreach (KeyValuePair<IPEndPoint, ClientData> client in clients)
            {
                if (Time.RealTimeSinceStartUp - client.Value.lastResponce > timeout)
                    toRemove.Add(client.Key);
            }

            foreach (IPEndPoint ip in toRemove)
            {
                ServerConsole.Log("Client timeout: " + ip);
                DisconectClient(ip);
            }
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

            foreach (KeyValuePair<IPEndPoint, Dictionary<uint, float>> userEntry in recivedAndUsedPacket)
            {
                foreach (KeyValuePair<uint, float> packetEntry in userEntry.Value)
                {
                    if (Time.RealTimeSinceStartUp - packetEntry.Value > 10)
                    {
                        toRemove.Add((userEntry.Key, packetEntry.Key));
                    }
                }
            }

            foreach ((IPEndPoint user, uint packetId) entry in toRemove)
            {
                if (recivedAndUsedPacket.TryGetValue(entry.user, out Dictionary<uint, float> packets))
                {
                    packets.Remove(entry.packetId);

                    if (packets.Count == 0)
                        recivedAndUsedPacket.Remove(entry.user);
                }
            }
        }
    }
}