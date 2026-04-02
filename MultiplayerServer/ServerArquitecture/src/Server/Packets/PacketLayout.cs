using KapNet;

namespace ServerArquitecture.src.Server.Packets
{
    internal static class PacketLayout
    {
        public const int PacketTypeOffSet = 0;
        public const int PacketIDOffSet = sizeof(PacketType);
        public const int PacketMetaDataOffSet = PacketIDOffSet + sizeof(int);
        public const int PacketDataOffSet = PacketMetaDataOffSet + sizeof(PacketMetaData);
        public const int CheckSum2EndOffSet = sizeof(int);
        public const int CheckSum1EndOffSet = CheckSum2EndOffSet * 2;
    }
}
