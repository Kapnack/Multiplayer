using ImageCampus.ToolBox.Dataflow;
using MultiplayerServer.src;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using ImageCampus.ToolBox.Services;
using ServerArquitecture.src;
using ServerArquitecture.src.Server.Packets;

namespace KapNet
{
    public class Server : IReceiveData, IInitable, ITickable, IDisposable
    {
        public int port = 7777;
        public int timeout = 10;

        private UdpConnection connection;
        Time Time => ServiceProvider.Instance.GetService<Time>();
        PacketFactory PacketFactory => ServiceProvider.Instance.GetService<PacketFactory>();

        private RSACryptoServiceProvider rsa;
        private string publicKey;

        private Dictionary<IPEndPoint, double> clients = new Dictionary<IPEndPoint, double>();

        private Dictionary<uint, uint> idsToIndex = new Dictionary<uint, uint>();

        private List<IPEndPoint> clientList = new List<IPEndPoint>();

        private uint id = 0;

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
            CheckTimeouts();
        }

        void Unload()
        {
            foreach (IPEndPoint client in new List<IPEndPoint>(clientList))
            {
                RemoveClient(client);
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
            byte[] payload = PacketUtility.GetPayload(data);

            clients[ip] = Time.RealTimeSinceStartUp;

            if (PacketUtility.CalculateCheckSum(data, 0, sizeof(int) * 2) != PacketUtility.GetCheckSum1(data) ||
                PacketUtility.CalculateCheckSum(data, 0, sizeof(int)) != PacketUtility.GetCheckSum2(data))
            {
                return;
            }

            switch (type)
            {
                case PacketType.Handshake:

                    if (clientList.Contains(ip))
                        return;

                    id++;
                    uint newID = id;

                    ServerConsole.Log("Client connected: " + ip + " ID: " + newID);

                    clientList.Add(ip);
                    uint index = (uint)(clientList.Count - 1);

                    idsToIndex[newID] = index;

                    Send(ip, PacketType.HandshakeResponse, BitConverter.GetBytes(newID));

                    foreach (KeyValuePair<uint, uint> existing in idsToIndex)
                    {
                        if (existing.Key == newID) continue;

                        Send(ip, PacketType.Spawn, BitConverter.GetBytes(existing.Key));
                    }

                    BroadcastWithException(
                        PacketFactory.Create(PacketType.Spawn, BitConverter.GetBytes(newID)),
                        ip
                    );

                    break;

                case PacketType.Ping:
                    Send(ip, PacketType.Pong);
                    break;

                case PacketType.Data:
                    Broadcast(data);
                    break;

                case PacketType.Disconnect:
                    RemoveClient(ip);
                    break;
            }
        }

        void Send(IPEndPoint ip, PacketType type, byte[] payload = null)
        {
            connection.Send(PacketFactory.Create(type, payload), ip);
        }

        void Broadcast(byte[] data)
        {
            foreach (IPEndPoint client in clientList)
            {
                connection.Send(data, client);
            }
        }

        void BroadcastWithException(byte[] data, IPEndPoint exception)
        {
            foreach (IPEndPoint client in clientList)
            {
                if (client.Equals(exception))
                    continue;

                connection.Send(data, client);
            }
        }

        void RemoveClient(IPEndPoint ip)
        {
            if (!clients.ContainsKey(ip))
                return;

            ServerConsole.Log("Client removed: " + ip);

            clients.Remove(ip);

            int index = clientList.IndexOf(ip);
            if (index == -1) return;

            clientList.RemoveAt(index);

            uint removedId = 0;

            foreach (KeyValuePair<uint, uint> kvp in idsToIndex)
            {
                if (kvp.Value == index)
                {
                    removedId = kvp.Key;
                    break;
                }
            }

            if (removedId != 0)
                idsToIndex.Remove(removedId);

            List<uint> keys = new List<uint>(idsToIndex.Keys);

            foreach (uint key in keys)
            {
                if (idsToIndex[key] > index)
                    idsToIndex[key]--;
            }

            Broadcast(PacketFactory.Create(
                PacketType.Disconnect,
                BitConverter.GetBytes(removedId)
            ));
        }

        void CheckTimeouts()
        {
            double now = Time.RealTimeSinceStartUp;
            List<IPEndPoint> toRemove = new List<IPEndPoint>();

            foreach (KeyValuePair<IPEndPoint, double> c in clients)
            {
                if (now - c.Value > timeout)
                    toRemove.Add(c.Key);
            }

            foreach (IPEndPoint ip in toRemove)
            {
                ServerConsole.Log("Client timeout: " + ip);
                RemoveClient(ip);
            }
        }
    }
}