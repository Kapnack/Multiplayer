using MultiplayerServer.src.Time;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace KapNet
{
    public class Server : IReceiveData, IDisposable
    {
        bool running = true;

        public int port = 7777;
        public int timeout = 10;

        private UdpConnection connection;
        Time time;

        private RSACryptoServiceProvider rsa;
        private string publicKey;

        private Dictionary<IPEndPoint, double> clients = new Dictionary<IPEndPoint, double>();

        private Dictionary<uint, uint> idsToIndex = new Dictionary<uint, uint>();

        private List<IPEndPoint> clientList = new List<IPEndPoint>();

        private uint id = 0;

        public void Run()
        {
            Init();
            Update();
            Dispose();
        }

        void Init()
        {
            time = new Time();

            connection = new UdpConnection(port, this);
            Console.WriteLine("Server started");

            rsa = new RSACryptoServiceProvider();
            publicKey = rsa.ToXmlString(false);
        }

        public void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            PacketType type = PacketBuilder.GetType(data);
            byte[] payload = PacketBuilder.GetPayload(data);

            clients[ip] = time.RealTimeSinceStartUp;

            switch (type)
            {
                case PacketType.Handshake:

                    if (clientList.Contains(ip))
                        return;

                    id++;
                    uint newID = id;

                    Console.WriteLine("Client connected: " + ip + " ID: " + newID);

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
                        PacketBuilder.Create(PacketType.Spawn, BitConverter.GetBytes(newID)),
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
            connection.Send(PacketBuilder.Create(type, payload), ip);
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

            Console.WriteLine("Client removed: " + ip);

            clients.Remove(ip);

            int index = clientList.IndexOf(ip);
            if (index == -1) return;

            clientList.RemoveAt(index);

            uint removedId = 0;

            foreach (var kvp in idsToIndex)
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

            foreach (var key in keys)
            {
                if (idsToIndex[key] > index)
                    idsToIndex[key]--;
            }

            // Notify others
            Broadcast(PacketBuilder.Create(
                PacketType.Disconnect,
                BitConverter.GetBytes(removedId)
            ));
        }

        void Update()
        {
            const int tickRate = 60;
            int delay = 1000 / tickRate;

            while (running)
            {
                int start = Environment.TickCount;

                connection?.FlushReceiveData();
                CheckTimeouts();

                int elapsed = Environment.TickCount - start;
                int sleepTime = delay - elapsed;

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("Shutting down...");
                        running = false;
                    }
                }

                if (sleepTime > 0)
                    Thread.Sleep(sleepTime);
            }
        }

        void CheckTimeouts()
        {
            double now = time.RealTimeSinceStartUp;
            List<IPEndPoint> toRemove = new List<IPEndPoint>();

            foreach (KeyValuePair<IPEndPoint, double> c in clients)
            {
                if (now - c.Value > timeout)
                    toRemove.Add(c.Key);
            }

            foreach (IPEndPoint ip in toRemove)
            {
                Console.WriteLine("Client timeout: " + ip);
                RemoveClient(ip);
            }
        }

        void Unload()
        {
            foreach (var client in new List<IPEndPoint>(clientList))
            {
                RemoveClient(client);
            }
        }

        public void Dispose()
        {
            Unload();
        }
    }
}