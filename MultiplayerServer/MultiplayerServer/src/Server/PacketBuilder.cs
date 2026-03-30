using System;

namespace KapNet
{
    public enum PacketType : byte
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

    public static class PacketBuilder
    {
        private const int PacketTypeOffSet = 0;
        private const int PacketIDOffSet = 1;
        private const int PacketDataOffSet = sizeof(uint) + 1;

        private static uint packetID = 0;

        public static byte[] Create(PacketType type, byte[] payload = null)
        {
            ++packetID;

            if (payload == null)
                payload = new byte[0];

            byte[] data = new byte[1 + sizeof(uint) + payload.Length + sizeof(int) * 2];

            data[PacketTypeOffSet] = Convert.ToByte(type);

            BitConverter.GetBytes(packetID).CopyTo(data, PacketIDOffSet);

            Buffer.BlockCopy(payload, 0, data, PacketDataOffSet, payload.Length);

            BitConverter.GetBytes(CalculateCheckSum(data, 0, sizeof(int) * 2)).CopyTo(data, data.Length - sizeof(int) * 2);
            BitConverter.GetBytes(CalculateCheckSum(data, 0, sizeof(int))).CopyTo(data, data.Length - sizeof(int));

            return data;
        }

        public static int CalculateCheckSum(byte[] data, int startOffset = 0, int endOffset = 0)
        {
            byte checksum = default(byte);

            for (int i = startOffset; i < data.Length - endOffset; ++i)
                checksum ^= data[i];

            return checksum;
        }

        public static PacketType GetType(byte[] data)
        {
            return (PacketType)data[PacketTypeOffSet];
        }

        public static uint GetID(byte[] data)
        {
            return BitConverter.ToUInt32(data, PacketIDOffSet);
        }

        public static int GetCheckSum1(byte[] data)
        {
            return BitConverter.ToInt32(data, data.Length - sizeof(int) * 2);
        }

        public static int GetCheckSum2(byte[] data)
        {
            return BitConverter.ToInt32(data, data.Length - sizeof(int));
        }

        public static byte[] GetPayload(byte[] data)
        {
            int payloadSize = data.Length - (PacketDataOffSet + sizeof(int) * 2);
            byte[] payload = new byte[payloadSize];

            Buffer.BlockCopy(data, PacketDataOffSet, payload, 0, payloadSize);

            return payload;
        }
    }
}