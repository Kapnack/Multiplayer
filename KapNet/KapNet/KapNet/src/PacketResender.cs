using KapNet.src.time;
using System.Collections.Generic;
using System.Net;

namespace KapNet.src
{
    internal class PacketResender
    {
        private Dictionary<PacketType, List<PacketAwaitingResponce>> packetsAwaitingResponce = new Dictionary<PacketType, List<PacketAwaitingResponce>>();

        private INetworkPeer networkPeer;

        public PacketResender(INetworkPeer networkPeer)
        {
            this.networkPeer = networkPeer;
        }

        public void Tick(double RealTimeSinceStartUp)
        {
            foreach (KeyValuePair<PacketType, List<PacketAwaitingResponce>> packetsType in packetsAwaitingResponce)
            {
                List<PacketAwaitingResponce> currentList = packetsType.Value;

                for (int i = 0; i < currentList.Count; ++i)
                {
                    PacketAwaitingResponce packet = currentList[i];

                    if (RealTimeSinceStartUp - packet.lastTimeSent > 3)
                    {
                        if (!networkPeer.IsConnected)
                            networkPeer.SendRaw(packet.data, packet.ipEndPoint);
                        else
                            networkPeer.SendRaw(packet.data);

                        packet.lastTimeSent = RealTimeSinceStartUp;
                    }
                }
            }
        }

        public void Add(PacketType packetType, PacketAwaitingResponce packet)
        {
            if (!packetsAwaitingResponce.ContainsKey(packetType))
                packetsAwaitingResponce[packetType] = new List<PacketAwaitingResponce>();

            packetsAwaitingResponce[packetType].Add(packet);
        }

        public void Add(PacketType packetType, uint packetID, byte[] data, double RealTimeSinceStartUp, IPEndPoint reciver = null)
        {
            if (!packetsAwaitingResponce.ContainsKey(packetType))
                packetsAwaitingResponce[packetType] = new List<PacketAwaitingResponce>();

            packetsAwaitingResponce[packetType].Add(new PacketAwaitingResponce(
              packetID,
              data,
              reciver,
              RealTimeSinceStartUp
          ));
        }

        public void Remove(PacketType packetType, uint packetID)
        {
            if (!packetsAwaitingResponce.ContainsKey(packetType))
                return;

            packetsAwaitingResponce[packetType].RemoveAll(p => p.packetID == packetID);
        }

        public void Clear()
        {
            packetsAwaitingResponce.Clear();
        }
    }
}
