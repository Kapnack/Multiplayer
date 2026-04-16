using ImageCampus.ToolBox.Dataflow;
using KapNet.src;
using ServerArquitecture.src;
using System;
using System.Collections.Generic;
using System.Net;

namespace KapNet
{
    public class ClientData
    {
        public uint id;
        public bool isConnected;
        public double lastResponce;
    }

    public class Server : NetworkPeer<IPEndPoint>, IInitable, ITickable, IDisposable
    {
        public int port = 7777;
        public int timeout = 10;

        IPEndPoint matchMakingIP;
        private double matchMakerLastResponce;

        private Dictionary<IPEndPoint, ClientData> clients = new Dictionary<IPEndPoint, ClientData>();

        private uint currentClientID = 0;

        private bool isConnectedToMatchMaking = false;

        internal Server(string matchMakingIP, int portToConnect, int portToHost) : base()
        {
            PacketTypeStrategy.Add(PacketType.Data, HandleData);

            Connect(portToHost);

            IPAddress ipAddress = IPAddress.Parse(matchMakingIP);
            this.matchMakingIP = new IPEndPoint(ipAddress, portToConnect);

            Send(this.matchMakingIP, PacketType.Handshake, BitConverter.GetBytes((byte)ConnectionRole.Server), PacketMetaData.Reliable);

            isConnectedToMatchMaking = true;
        }

        internal Server() : base()
        {
            PacketTypeStrategy.Add(PacketType.Data, HandleData);

            Connect(7777);
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

            if (Time.RealTimeSinceStartUp - matchMakerLastResponce > timeout)
                isConnectedToMatchMaking = false;
        }

        private void SendPingToMatchMaker()
        {
            Send(matchMakingIP, PacketType.Ping, BitConverter.GetBytes(Time.RealTimeSinceStartUp));
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

            if (ip.Equals(matchMakingIP))
                matchMakerLastResponce = Time.RealTimeSinceStartUp;
            else if (clients.ContainsKey(ip))
                clients[ip].lastResponce = Time.RealTimeSinceStartUp;

            Send(ip, PacketType.Ping);
        }

        private void HandleData(NetworkPacket packet)
        {
            BroadcastRaw(packet.data, packet.ipEndPoint);
        }

        protected override void HandleHandShake(NetworkPacket networkPacket)
        {
            if (clients.ContainsKey(networkPacket.ipEndPoint))
            {
                Send(networkPacket.ipEndPoint, PacketType.Handshake, BitConverter.GetBytes(clients[networkPacket.ipEndPoint].id), PacketMetaData.Reliable);
                clients[networkPacket.ipEndPoint].isConnected = true;
                clients[networkPacket.ipEndPoint].lastResponce = Time.RealTimeSinceStartUp;
                return;
            }

            ++currentClientID;
            uint newID = currentClientID;

            lock (clients)
                clients.Add(networkPacket.ipEndPoint, new ClientData
                {
                    id = newID,
                    isConnected = true,
                    lastResponce = Time.RealTimeSinceStartUp
                });

            ServerConsole.Log($"Client connected: {networkPacket.ipEndPoint} ID: {newID}");

            Send(networkPacket.ipEndPoint, PacketType.Handshake, BitConverter.GetBytes(newID), PacketMetaData.Reliable);

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
                    if (Time.RealTimeSinceStartUp - client.Value.lastResponce > timeout)
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