using ImageCampus.ToolBox.Services;
using ServerArquitecture.src.Server.Packets;
using System;

namespace KapNet
{
    public enum PacketType : int
    {
        Handshake,
        Acknowledgement,
        SendID,
        ClientJoined,
        ClientLeft,
        Ping,
        Data,
        DisconnectClient,
        ServerShutDown,
        ConnectToServer
    }

    [Flags]
    public enum PacketMetaData : int
    {
        None = 1 << 0,
        Crytical = 1 << 1,
        Reliable = 1 << 2,
        Encrypted = 1 << 3,
        Ordenable = 1 << 4
    }

    public enum ConnectionRole : byte
    {
        Client = 0,
        Server = 1
    }

    internal class PacketFactory : IService
    {
        private uint packetID = 0;

        public bool IsPersistance => true;

        public PacketFactory()
        { }

        public (byte[] data, uint packetId) Create(PacketType type, byte[] payload = null, PacketMetaData metaData = PacketMetaData.None)
        {
            ++packetID;

            if (payload == null)
                payload = new byte[0];

            byte[] data = new byte[PacketLayout.PacketConstSpace + payload.Length];

            BitConverter.GetBytes((int)type).CopyTo(data, PacketLayout.PacketTypeOffSet);
            BitConverter.GetBytes(packetID).CopyTo(data, PacketLayout.PacketIDOffSet);
            BitConverter.GetBytes((int)metaData).CopyTo(data, PacketLayout.PacketMetaDataOffSet);

            Buffer.BlockCopy(payload, 0, data, PacketLayout.PacketPayloadOffSet, payload.Length);

            BitConverter.GetBytes(PacketUtility.CalculateCheckSum(data, 0, PacketLayout.CheckSum1EndOffSet)).CopyTo(data, data.Length - PacketLayout.CheckSum1EndOffSet);
            BitConverter.GetBytes(PacketUtility.CalculateCheckSum(data, 0, PacketLayout.CheckSum2EndOffSet)).CopyTo(data, data.Length - PacketLayout.CheckSum2EndOffSet);

            return (data, packetID);
        }
    }
}