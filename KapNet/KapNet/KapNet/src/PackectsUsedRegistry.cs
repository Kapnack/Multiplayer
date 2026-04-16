using System.Collections.Generic;
using System.Net.Sockets;

namespace KapNet.src
{
    internal class PackectsUsedRegistry<ClientKeyType, PacketsKeyType>
    {
        private Dictionary<ClientKeyType, Dictionary<PacketsKeyType, Dictionary<uint, double>>> recivedAndUsedPacket = new Dictionary<ClientKeyType, Dictionary<PacketsKeyType, Dictionary<uint, double>>>();

        List<(ClientKeyType user, PacketsKeyType packetType, uint packetId)> toRemove = new List<(ClientKeyType user, PacketsKeyType packetType, uint packetId)>();

        public void Tick(double RealTimeSinceStartUp)
        {
            foreach (KeyValuePair<ClientKeyType, Dictionary<PacketsKeyType, Dictionary<uint, double>>> userPacketPerType in recivedAndUsedPacket)
            {
                ClientKeyType userID = userPacketPerType.Key;

                foreach (KeyValuePair<PacketsKeyType, Dictionary<uint, double>> typePerID in userPacketPerType.Value)
                {
                    PacketsKeyType type = typePerID.Key;

                    foreach (KeyValuePair<uint, double> packet in typePerID.Value)
                    {
                        if (RealTimeSinceStartUp - packet.Value > 10)
                        {
                            toRemove.Add((userID, type, packet.Key));
                        }
                    }
                }
            }

            foreach ((ClientKeyType userID, PacketsKeyType type, uint packetId) entry in toRemove)
            {
                if (recivedAndUsedPacket.TryGetValue(entry.userID, out var typeDict))
                {
                    if (typeDict.TryGetValue(entry.type, out var packetDict))
                    {
                        packetDict.Remove(entry.packetId);

                        if (packetDict.Count == 0)
                            typeDict.Remove(entry.type);
                    }

                    if (typeDict.Count == 0)
                        recivedAndUsedPacket.Remove(entry.userID);
                }
            }

            if (toRemove.Count > 0)
                toRemove.Clear();
        }

        public bool ContainsPacket(ClientKeyType user, PacketsKeyType packetType, uint packetId)
        {
            if (!recivedAndUsedPacket.TryGetValue(user, out Dictionary<PacketsKeyType, Dictionary<uint, double>> userTypePackets))
                return false;

            if (!userTypePackets.TryGetValue(packetType, out Dictionary<uint, double> packets))
                return false;

            if (!packets.ContainsKey(packetId))
                return false;

            return true;
        }

        public void SetPacket(ClientKeyType clientKey, PacketsKeyType packetType, uint packetID, double RealTimeSinceStartUp)
        {
            if (!recivedAndUsedPacket.TryGetValue(clientKey, out Dictionary<PacketsKeyType, Dictionary<uint, double>> packetsPerType))
            {
                packetsPerType = new Dictionary<PacketsKeyType, Dictionary<uint, double>>();
                recivedAndUsedPacket[clientKey] = packetsPerType;
            }

            if (!packetsPerType.TryGetValue(packetType, out Dictionary<uint, double> packetsById))
            {
                packetsById = new Dictionary<uint, double>();
                packetsPerType[packetType] = packetsById;
            }

            packetsById[packetID] = RealTimeSinceStartUp;
        }

        public void Clear()
        {
            recivedAndUsedPacket.Clear();
            toRemove.Clear();
        }
    }
}
