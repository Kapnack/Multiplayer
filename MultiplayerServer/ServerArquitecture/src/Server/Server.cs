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
        PacketFactory PacketFactory => ServiceProvider.Instance.GetService<PacketFactory>();

        private RSACryptoServiceProvider rsa;
        private string publicKey;

        private Dictionary<IPEndPoint, ClientData> clients = new Dictionary<IPEndPoint, ClientData>();

        private Dictionary<uint, NetworkPacket> recivedAndUsedPacket = new Dictionary<uint, NetworkPacket>();
        private List<PacketAwaitingResponce> packetsAwaitingResponce = new List<PacketAwaitingResponce>();
        private List<NetworkPacket> cryticalPackets = new List<NetworkPacket>();

        private readonly Dictionary<PacketType, PacketTypeDelegate> packetTypeStrategy;
        private readonly Dictionary<PacketMetaData, SendPacketMetaDataDelegate> sendingMetaDataStrategy;
        private readonly Dictionary<PacketMetaData, RecivePacketMetaDataDelegate> recivingMetaDataStrategy;

        private uint currentClientID = 0;

        internal Server()
        {
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
                {PacketMetaData.Crytical, HandleCriticalPacketRecived }
            };
        }

        private void HandleCriticalPacketRecived(NetworkPacket packet)
        {
            cryticalPackets.Add(packet);
        }

        private void HandleAcknowledgement(NetworkPacket networkPacket)
        {
            uint packetID = BitConverter.ToUInt32(networkPacket.payload, 0);
            packetsAwaitingResponce.RemoveAll(p => p.packetID == packetID);
        }

        private void HandleReliablePacketRecived(NetworkPacket packet)
        {
            Send(packet.ipEndPoint, PacketType.Acknowledgement, BitConverter.GetBytes(packet.packetID));
            recivedAndUsedPacket[packet.packetID] = packet;
        }

        public void Init()
        {
            connection = new UdpConnection(port, this);
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

            if (recivedAndUsedPacket.ContainsKey(packetID))
            {
                recivedAndUsedPacket[packetID].timeStamp = (float)Time.RealTimeSinceStartUp;
                HandleAcknowledgement(networkPacket);
                return;
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
                clients[ip].lastResponce = Time.RealTimeSinceStartUp;

            Send(ip, PacketType.Pong);
        }

        private void HandleData(NetworkPacket packet)
        {
            byte[] newPayload = PacketUtility.Combine(
                BitConverter.GetBytes(packet.clientId),
                packet.payload
            );

            Broadcast(packet.ipEndPoint, PacketType.Data, newPayload, packet.metaData);
        }

        private void HandleHandShake(NetworkPacket packet)
        {
            if (clients.ContainsKey(packet.ipEndPoint))
                return;

            ++currentClientID;
            uint newID = currentClientID;

            clients.Add(packet.ipEndPoint, new ClientData
            {
                id = newID,
                lastResponce = Time.RealTimeSinceStartUp
            });

            ServerConsole.Log($"Client connected: {packet.ipEndPoint} ID: {newID}");

            Send(packet.ipEndPoint, PacketType.SendID, BitConverter.GetBytes(packet.packetID), PacketMetaData.Reliable);
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

            NetworkPacket packet = new NetworkPacket(
                type,
                packetId,
                metaData,
                payload,
                (float)Time.RealTimeSinceStartUp,
                clients.ContainsKey(ip) ? (int)clients[ip].id : -1,
                ip
            );

            HandleSendMetaData(packet, data);

            SendRaw(data, ip);
        }

        void SendRaw(byte[] data, IPEndPoint ip)
        {
            connection.Send(data, ip);
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

            Broadcast(ip, PacketType.DisconnectClient, BitConverter.GetBytes(clients[ip].id), PacketMetaData.Reliable);

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
                    connection.Send(packet.data, packet.ipEndPoint);

                    packet.lastTimeSent = Time.RealTimeSinceStartUp;
                }
            }
        }

        void CheckDiscartOfRecivedAndUsed()
        {
            List<uint> toRemove = new List<uint>();

            foreach (KeyValuePair<uint, NetworkPacket> networkPacket in recivedAndUsedPacket)
            {
                if (Time.RealTimeSinceStartUp - networkPacket.Value.timeStamp > 10)
                    toRemove.Add(networkPacket.Key);
            }

            foreach (uint packetID in toRemove)
                recivedAndUsedPacket.Remove(packetID);
        }
    }
}