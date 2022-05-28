using System;
using System.Runtime.InteropServices;

namespace Saro
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IDStruct
    {
        public uint Time;    // 30bit
        public int Process;  // 18bit
        public ushort Value; // 16bit

        public long ToLong()
        {
            ulong result = 0;
            result |= Value;
            result |= (ulong)Process << 16;
            result |= (ulong)Time << 34;
            return (long)result;
        }

        public IDStruct(uint time, int process, ushort value)
        {
            Process = process;
            Time = time;
            Value = value;
        }

        public IDStruct(long id)
        {
            ulong result = (ulong)id;
            Value = (ushort)(result & ushort.MaxValue);
            result >>= 16;
            Process = (int)(result & IDGenerater.Mask18bit);
            result >>= 18;
            Time = (uint)result;
        }

        public override string ToString()
        {
            return $"process: {Process}, time: {Time}, value: {Value}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstanceIDStruct
    {
        public uint Time;   // 当年开始的tick 28bit
        public int Process; // 18bit
        public uint Value;  // 18bit

        public long ToLong()
        {
            ulong result = 0;
            result |= Value;
            result |= (ulong)Process << 18;
            result |= (ulong)Time << 36;
            return (long)result;
        }

        public InstanceIDStruct(long id)
        {
            ulong result = (ulong)id;
            Value = (uint)(result & IDGenerater.Mask18bit);
            result >>= 18;
            Process = (int)(result & IDGenerater.Mask18bit);
            result >>= 18;
            Time = (uint)result;
        }

        public InstanceIDStruct(uint time, int process, uint value)
        {
            Time = time;
            Process = process;
            Value = value;
        }

        // 给SceneId使用
        public InstanceIDStruct(int process, uint value)
        {
            Time = 0;
            Process = process;
            Value = value;
        }

        public override string ToString()
        {
            return $"process: {Process}, value: {Value} time: {Time}";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UnitIDStruct
    {
        public uint Time;        // 30bit 34年
        public ushort Zone;      // 10bit 1024个区
        public byte ProcessMode; // 8bit  Process % 256  一个区最多256个进程
        public ushort Value;     // 16bit 每秒每个进程最大16K个Unit

        public long ToLong()
        {
            ulong result = 0;

            result |= 1ul << 63; // 最高位变成1，暂时让它跟普通id区分一下，正式版删除

            result |= Value;
            result |= (uint)ProcessMode << 16;
            result |= (ulong)Zone << 24;
            result |= (ulong)Time << 34;
            return (long)result;
        }

        public UnitIDStruct(int zone, int process, uint time, ushort value)
        {
            Time = time;
            ProcessMode = (byte)(process % 256);
            Value = value;
            Zone = (ushort)zone;
        }

        public override string ToString()
        {
            return $"ProcessMode: {ProcessMode}, value: {Value} time: {Time}";
        }

        public static int GetUnitZone(long unitId)
        {
            int v = (int)((unitId >> 24) & 0x03ff); // 取出10bit
            return v;
        }
    }

    public class IDGenerater : IDisposable
    {
        public const int Mask18bit = 0x03ffff;
        public static IDGenerater Instance = new IDGenerater();

        public const int MaxZone = 1024;

        private long epoch2020;
        private ushort value;
        private uint lastIdTime;
        private ushort idThisSecCount;


        private long instanceIdEpoch;
        private uint instanceIdValue;
        private uint lastInstanceIdTime;
        private uint instanceIdThisSecCount;


        private ushort unitIdValue;
        private uint lastUnitIdTime;
        private ushort unitIdThisSecCount;

        public IDGenerater()
        {
            long epoch1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            epoch2020 = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970;
            instanceIdEpoch = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970;
        }

        public void Dispose()
        {
            epoch2020 = 0;
            instanceIdEpoch = 0;
            value = 0;
        }

        private uint TimeSince2020()
        {
            return (uint)((FGame.TimeInfo.FrameTime - epoch2020) / 1000);
        }

        private uint TimeSinceThisYear()
        {
            return (uint)((FGame.TimeInfo.FrameTime - instanceIdEpoch) / 1000);
        }

        public long GenerateInstanceID()
        {
            uint time = TimeSinceThisYear();

            if (time == lastInstanceIdTime)
            {
                ++instanceIdThisSecCount;
            }
            else
            {
                lastInstanceIdTime = time;
                instanceIdThisSecCount = 1;
            }
            if (instanceIdThisSecCount > IDGenerater.Mask18bit - 1)
            {
                Log.ERROR($"instanceid count per sec overflow: {instanceIdThisSecCount}");
            }


            if (++instanceIdValue > IDGenerater.Mask18bit - 1) // 18bit
            {
                instanceIdValue = 0;
            }
            InstanceIDStruct instanceIdStruct = new InstanceIDStruct(time, FGame.s_Options.Process, instanceIdValue);
            return instanceIdStruct.ToLong();
        }

        public long GenerateID()
        {
            uint time = TimeSince2020();

            if (time == lastIdTime)
            {
                ++idThisSecCount;
            }
            else
            {
                lastIdTime = time;
                idThisSecCount = 1;
            }
            if (idThisSecCount == ushort.MaxValue)
            {
                Log.ERROR($"id count per sec overflow: {idThisSecCount}");
            }


            if (++value > ushort.MaxValue - 1)
            {
                value = 0;
            }
            IDStruct idStruct = new IDStruct(time, FGame.s_Options.Process, value);
            return idStruct.ToLong();
        }

        public long GenerateUnitId(int zone)
        {
            if (zone > MaxZone)
            {
                throw new Exception($"zone > MaxZone: {zone}");
            }
            uint time = TimeSince2020();


            if (time == lastUnitIdTime)
            {
                ++unitIdThisSecCount;
            }
            else
            {
                lastUnitIdTime = time;
                unitIdThisSecCount = 1;
            }
            if (unitIdThisSecCount == ushort.MaxValue)
            {
                Log.ERROR($"unitid count per sec overflow: {unitIdThisSecCount}");
            }

            if (++unitIdValue > ushort.MaxValue - 1)
            {
                unitIdValue = 0;
            }

            UnitIDStruct unitIdStruct = new UnitIDStruct(zone, FGame.s_Options.Process, time, unitIdValue);
            return unitIdStruct.ToLong();
        }
    }
}