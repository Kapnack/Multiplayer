using ImageCampus.ToolBox.Services;
using ServerArquitecture.src.Server.Packets;
using System;

namespace Assets.Code.Network.packets
{
    public enum PacketType : int
    {
        Handshake,
        HandshakeResponse,
        Spawn,
        PlayerJoined,
        PlayerLeft,
        Ping,
        Pong,
        Data,
        Disconnect
    }

    [Flags]
    public enum PacketMetaData : int
    {
        None = 1 << 0,
        Crytical = 1 << 1,
        Reliable = 1 << 2,
        Encrypted = 1 << 3
    }

    internal class PacketFactory : IService
    {
        private uint packetID = 0;

        public bool IsPersistance => true;

        private const int PacketConstSpace = sizeof(PacketType) + sizeof(int) + sizeof(PacketMetaData) + sizeof(int) * 2;

        public byte[] Create(PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
        {
            ++packetID;

            if (payload == null)
                payload = new byte[0];

            byte[] data = new byte[PacketConstSpace + payload.Length];

            BitConverter.GetBytes((int)type).CopyTo(data, PacketLayout.PacketTypeOffSet);
            BitConverter.GetBytes(packetID).CopyTo(data, PacketLayout.PacketIDOffSet);
            BitConverter.GetBytes((int)metaData).CopyTo(data, PacketLayout.PacketMetaDataOffSet);

            Buffer.BlockCopy(payload, 0, data, PacketLayout.PacketDataOffSet, payload.Length);

            BitConverter.GetBytes(PacketUtility.CalculateCheckSum(data, 0, PacketLayout.CheckSum1EndOffSet)).CopyTo(data, data.Length - PacketLayout.CheckSum1EndOffSet);
            BitConverter.GetBytes(PacketUtility.CalculateCheckSum(data, 0, PacketLayout.CheckSum2EndOffSet)).CopyTo(data, data.Length - PacketLayout.CheckSum2EndOffSet);

            return data;
        }
    }
}