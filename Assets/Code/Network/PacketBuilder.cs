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
        private static uint packetID = 0;

        public static byte[] Create(PacketType type, byte[] payload = null)
        {
            ++packetID;

            if (payload == null)
                payload = new byte[0];

            byte[] data = new byte[1 + sizeof(uint) + payload.Length + 2];

            data[0] = Convert.ToByte(type);

            BitConverter.GetBytes(packetID).CopyTo(data, 1);

            Buffer.BlockCopy(payload, 0, data, 1, payload.Length);

            data[data.Length - 2] = Convert.ToByte(CalculateCheckSum(data, 0, 2));
            data[data.Length - 1] = Convert.ToByte(CalculateCheckSum(data, 0, 1));

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
            return (PacketType)data[0];
        }

        public static byte[] GetPayload(byte[] data)
        {
            byte[] payload = new byte[data.Length - 1];
            Buffer.BlockCopy(data, 1, payload, 0, payload.Length);
            return payload;
        }
    }
}