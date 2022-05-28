using System.IO;

namespace Saro.Net
{
    public struct Packet
    {
        public const int MinPacketSize = 2;
        public const int OpcodeIndex = 8;
        public const int KcpOpcodeIndex = 0;
        public const int OpcodeLength = 2;
        public const int ActorIdIndex = 0;
        public const int ActorIdLength = 8;
        public const int MessageIndex = 10;

        public ushort Opcode;
        public long ActorId;
        public MemoryStream MemoryStream;
    }
}