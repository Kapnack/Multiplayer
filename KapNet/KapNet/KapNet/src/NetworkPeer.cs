using KapNet.src.time;
using System;
using System.Collections.Generic;
using System.Net;

namespace KapNet.src
{
    public abstract class NetworkPeer<ClientKey> : IReceiveData, INetworkPeer
    {
        protected delegate void PacketTypeDelegate(NetworkPacket networkPacket);
        private delegate void SendPacketMetaDataDelegate(NetworkPacket networkPacket);
        private delegate void RecivePacketMetaDataDelegate(NetworkPacket networkPacket);

        PacketResender packetResender;

        private List<NetworkPacket> cryticalPackets = new List<NetworkPacket>();

        private PackectsUsedRegistry<IPEndPoint, PacketType> packectsUsedRegistry = new PackectsUsedRegistry<IPEndPoint, PacketType>();

        protected Dictionary<PacketType, PacketTypeDelegate> PacketTypeStrategy { get; private set; }

        public bool IsConnected { get; private set; }

        private Dictionary<PacketMetaData, SendPacketMetaDataDelegate> sendingMetaDataStrategy;
        private Dictionary<PacketMetaData, RecivePacketMetaDataDelegate> recivingMetaDataStrategy;

        private PacketFactory packetFactory = new PacketFactory();

        private UdpConnection connection;

        protected Time Time = new Time();

        public NetworkPeer()
        {
            packetResender = new PacketResender(this);

            IsConnected = false;

            PacketTypeStrategy = new Dictionary<PacketType, PacketTypeDelegate>()
            {
                { PacketType.Handshake, HandleHandShake },
                { PacketType.Ping, HandlePing },
                { PacketType.ClientLeft, HandleClientLeft },
                { PacketType.Acknowledgement, HandleAcknowledgement }
            };

            sendingMetaDataStrategy = new Dictionary<PacketMetaData, SendPacketMetaDataDelegate>()
            {
                { PacketMetaData.Reliable, HandleReliableMessageSend },
                { PacketMetaData.Crytical, HandleCriticalMessageSend }
            };

            recivingMetaDataStrategy = new Dictionary<PacketMetaData, RecivePacketMetaDataDelegate>()
            {
                {PacketMetaData.Reliable, HandleReliablePacketRecived },
                {PacketMetaData.Ordenable, HandleOrdenablePacketRecived },
                {PacketMetaData.Crytical, HandleCriticalPacketRecived }
            };
        }

        public virtual void Tick()
        {
            if (connection != null)
                connection.FlushReceiveData();

            packetResender.Tick(Time.RealTimeSinceStartUp);
            packectsUsedRegistry.Tick(Time.RealTimeSinceStartUp);
        }

        public void Connect(string ip, int port)
        {
            packectsUsedRegistry.Clear();
            packetResender.Clear();

            IsConnected = true;

            if (connection != null)
                connection.Close();

            IPAddress iPAddress = IPAddress.Parse(ip);
            connection = new UdpConnection(iPAddress, port, this);
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            packectsUsedRegistry.Clear();
            packetResender.Clear();

            IsConnected = true;

            if (connection != null)
                connection.Close();

            connection = new UdpConnection(ipAddress, port, this);
        }

        public void Connect(int port)
        {
            packectsUsedRegistry.Clear();
            packetResender.Clear();

            if (connection != null)
                connection.Close();

            connection = new UdpConnection(port, this);
        }

        public virtual void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
        {
            PacketType type = PacketUtility.GetType(data);
            uint packetID = PacketUtility.GetPacketID(data);
            PacketMetaData metaData = PacketUtility.GetMetaData(data);
            byte[] payload = PacketUtility.GetPayload(data);

            NetworkPacket networkPacket = new NetworkPacket
            (
                data,
                type,
                packetID,
                metaData,
                payload,
                (float)Time.RealTimeSinceStartUp,
                -1,
                ipEndpoint
            );

            if (packectsUsedRegistry.ContainsPacket(ipEndpoint, type, packetID))
            {
                HandleReliablePacketRecived(networkPacket);
                return;
            }

            HandleRecivedMetaData(networkPacket);

            if (PacketTypeStrategy.TryGetValue(networkPacket.type, out PacketTypeDelegate handler))
                handler(networkPacket);
        }

        public void Send(IPEndPoint ip, PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
        {
            (byte[] data, uint packetId) = packetFactory.Create(type, payload, metaData);

            NetworkPacket networkPacket = new NetworkPacket
            (
                data,
                type,
                packetId,
                metaData,
                payload,
                (float)Time.RealTimeSinceStartUp,
                -1,
                ip
            );

            HandleSendMetaData(networkPacket);

            SendRaw(data, ip);
        }

        public void Send(PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
        {
            (byte[] data, uint packetId) = packetFactory.Create(type, payload, metaData);

            NetworkPacket networkPacket = new NetworkPacket
            (
                data,
                type,
                packetId,
                metaData,
                payload,
                (float)Time.RealTimeSinceStartUp
            );

            HandleSendMetaData(networkPacket);

            SendRaw(data);
        }

        private void HandleRecivedMetaData(NetworkPacket packet)
        {
            foreach (KeyValuePair<PacketMetaData, RecivePacketMetaDataDelegate> strategy in recivingMetaDataStrategy)
            {
                if (packet.metaData.HasFlag(strategy.Key))
                    strategy.Value(packet);
            }
        }

        private void HandleSendMetaData(NetworkPacket packet)
        {
            foreach (KeyValuePair<PacketMetaData, SendPacketMetaDataDelegate> strategy in sendingMetaDataStrategy)
            {
                if (packet.metaData.HasFlag(strategy.Key))
                {
                    strategy.Value(packet);
                }
            }
        }

        public void SendRaw(byte[] data, IPEndPoint ip)
        {
            connection.Send(data, ip);
        }

        public void SendRaw(byte[] data)
        {
            connection.Send(data);
        }

        protected void Disconnect()
        {
            connection.Close();
            connection = null;
            IsConnected = false;
        }

        protected abstract void HandleHandShake(NetworkPacket networkPacket);
        protected abstract void HandlePing(NetworkPacket networkPacket);
        protected abstract void HandleClientLeft(NetworkPacket networkPacket);

        private void HandleAcknowledgement(NetworkPacket networkPacket)
        {
            PacketType packetType = (PacketType)BitConverter.ToInt32(networkPacket.payload, 0);
            uint packetID = BitConverter.ToUInt32(networkPacket.payload, sizeof(PacketType));
            packetResender.Remove(packetType, packetID);
        }

        private void HandleReliableMessageSend(NetworkPacket networkPacket)
        {
            packetResender.Add(networkPacket.type, networkPacket.packetID, networkPacket.data, Time.RealTimeSinceStartUp, networkPacket.ipEndPoint);
        }

        private void HandleCriticalMessageSend(NetworkPacket packet)
        {

        }

        private void HandleReliablePacketRecived(NetworkPacket networkPacket)
        {
            byte[] payload = new byte[sizeof(PacketType) + sizeof(uint)];

            BitConverter.GetBytes((int)networkPacket.type).CopyTo(payload, 0);
            BitConverter.GetBytes(networkPacket.packetID).CopyTo(payload, sizeof(int));

            if (IsConnected)
                Send(PacketType.Acknowledgement, payload);
            else
                Send(networkPacket.ipEndPoint, PacketType.Acknowledgement, payload);

            packectsUsedRegistry.SetPacket(networkPacket.ipEndPoint, networkPacket.type, networkPacket.packetID, Time.RealTimeSinceStartUp);
        }

        private void HandleOrdenablePacketRecived(NetworkPacket networkPacket)
        {

        }

        private void HandleCriticalPacketRecived(NetworkPacket networkPacket)
        {

        }
    }
}
