using KapNet;
using System;

namespace ServerArquitecture.src.Server.Packets
{
    internal static class PacketUtility
    {
        public static int CalculateCheckSum(byte[] data, int startOffset = 0, int endOffset = 0)
        {
            int checksum = 0;

            for (int i = startOffset; i < data.Length - endOffset; ++i)
                checksum ^= data[i] << (i & 3) << 3;

            return checksum;
        }

        public static PacketType GetType(byte[] data)
        {
            return (PacketType)BitConverter.ToUInt32(data, PacketLayout.PacketTypeOffSet);
        }

        public static uint GetID(byte[] data)
        {
            return BitConverter.ToUInt32(data, PacketLayout.PacketIDOffSet);
        }

        public static int GetCheckSum1(byte[] data)
        {
            return BitConverter.ToInt32(data, data.Length - PacketLayout.CheckSum1EndOffSet);
        }

        public static int GetCheckSum2(byte[] data)
        {
            return BitConverter.ToInt32(data, data.Length - PacketLayout.CheckSum2EndOffSet);
        }

        public static byte[] GetPayload(byte[] data)
        {
            int payloadSize = data.Length - (PacketLayout.PacketDataOffSet + sizeof(int) * 2);
            byte[] payload = new byte[payloadSize];

            Buffer.BlockCopy(data, PacketLayout.PacketDataOffSet, payload, 0, payloadSize);

            return payload;
        }
    }
}
