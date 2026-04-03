using ServerArquitecture.src.Server.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace KapNet
{
    public class UdpConnection
    {
        private struct DataReceived
        {
            public byte[] data;
            public IPEndPoint ipEndPoint;
        }

        private readonly UdpClient connection;
        private IReceiveData receiver = null;
        private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

        object handler = new object();

        public UdpConnection(int port, IReceiveData receiver = null)
        {
            connection = new UdpClient(port);

            this.receiver = receiver;

            connection.BeginReceive(OnReceive, null);
        }

        public UdpConnection(IPAddress ip, int port, IReceiveData receiver = null)
        {
            connection = new UdpClient();
            connection.Connect(ip, port);

            this.receiver = receiver;

            connection.BeginReceive(OnReceive, null);
        }

        public void Close()
        {
            connection.Close();
        }

        public void FlushReceiveData()
        {
            lock (handler)
            {
                while (dataReceivedQueue.Count > 0)
                {
                    DataReceived dataReceived = dataReceivedQueue.Dequeue();
                    if (receiver != null)
                        receiver.OnReceiveData(dataReceived.data, dataReceived.ipEndPoint);
                }
            }
        }

        void OnReceive(IAsyncResult ar)
        {
            try
            {
                DataReceived dataReceived = new DataReceived();
                dataReceived.data = connection.EndReceive(ar, ref dataReceived.ipEndPoint);

                if (IsValidCheckSum(dataReceived.data))
                {
                    return;
                }

                lock (handler)
                {
                    dataReceivedQueue.Enqueue(dataReceived);
                }
            }
            catch (SocketException e)
            {
                //ServerConsole.Error("[UdpConnection] " + e.Message);
            }
            catch (Exception e)
            {
                //ServerConsole.Error("An unexpected error occurred: " + e.Message);
            }
            finally
            {
                connection.BeginReceive(OnReceive, null);
            }
        }

        private bool IsValidCheckSum(byte[] data)
        {
            return PacketUtility.CalculateCheckSum(data, 0, PacketLayout.CheckSum1EndOffSet) == PacketUtility.GetCheckSum1(data) &&
                PacketUtility.CalculateCheckSum(data, 0, PacketLayout.CheckSum2EndOffSet) == PacketUtility.GetCheckSum2(data);
        }

        public void Send(byte[] data)
        {
            connection.Send(data, data.Length);
        }

        public void Send(byte[] data, IPEndPoint ipEndPoint)
        {
            connection.Send(data, data.Length, ipEndPoint);
        }
    }
}