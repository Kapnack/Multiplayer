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
        public static byte[] Create(PacketType type, byte[] payload = null)
        {
            if (payload == null)
                payload = new byte[0];

            // First byte = packet type, rest = payload
            byte[] data = new byte[1 + payload.Length];
            data[0] = (byte)type;

            Buffer.BlockCopy(payload, 0, data, 1, payload.Length);
            return data;
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