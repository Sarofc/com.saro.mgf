using System;
using System.IO;

namespace Saro.Net
{
    public enum ParserState
    {
        PacketSize,
        PacketBody
    }

    public class PacketParser
    {
        private readonly CircularBuffer buffer;
        private int packetSize;
        private ParserState state;
        public AService service;
        private readonly byte[] cache = new byte[8];
        public const int InnerPacketSizeLength = 4;
        public const int OuterPacketSizeLength = 2;
        public MemoryStream MemoryStream;

        public PacketParser(CircularBuffer buffer, AService service)
        {
            this.buffer = buffer;
            this.service = service;
        }

        public bool Parse()
        {
            while (true)
            {
                switch (state)
                {
                    case ParserState.PacketSize:
                        {
                            if (service.ServiceType == ServiceType.Inner)
                            {
                                if (buffer.Length < InnerPacketSizeLength)
                                {
                                    return false;
                                }

                                buffer.Read(cache, 0, InnerPacketSizeLength);

                                packetSize = BitConverter.ToInt32(cache, 0);
                                if (packetSize > ushort.MaxValue * 16 || packetSize < Packet.MinPacketSize)
                                {
                                    throw new Exception($"recv packet size error, 可能是外网探测端口: {packetSize}");
                                }
                            }
                            else
                            {
                                if (buffer.Length < OuterPacketSizeLength)
                                {
                                    return false;
                                }

                                buffer.Read(cache, 0, OuterPacketSizeLength);

                                packetSize = BitConverter.ToUInt16(cache, 0);

                                System.Console.WriteLine("parse packetSize: " + packetSize);

                                if (packetSize < Packet.MinPacketSize)
                                {
                                    throw new Exception($"recv packet size error, 可能是外网探测端口: {packetSize}");
                                }
                            }

                            state = ParserState.PacketBody;
                            break;
                        }
                    case ParserState.PacketBody:
                        {
                            if (buffer.Length < packetSize)
                            {
                                return false;
                            }

                            MemoryStream memoryStream = MemoryStreamPool.Rent(packetSize);
                            buffer.Read(memoryStream, packetSize);
                            //memoryStream.SetLength(this.packetSize - Packet.MessageIndex);

                            if (MemoryStream != null)
                            {
                                MemoryStream.Dispose();
                            }
                            MemoryStream = memoryStream;

                            if (service.ServiceType == ServiceType.Inner)
                            {
                                memoryStream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
                            }
                            else
                            {
                                memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
                            }

                            state = ParserState.PacketSize;
                            return true;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}