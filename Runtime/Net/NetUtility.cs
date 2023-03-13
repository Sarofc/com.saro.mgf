using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.Net
{
    public static partial class NetUtility
    {
        private static readonly double[] k_ByteUnits =
        {
            1073741824.0, 1048576.0, 1024.0, 1
        };

        private static readonly string[] k_ByteUnitsNames =
        {
            "GB", "MB", "KB", "B"
        };

        public static string FormatBytes(long bytes)
        {
            var size = "0 B";
            if (bytes == 0) return size;

            for (var index = 0; index < k_ByteUnits.Length; index++)
            {
                var unit = k_ByteUnits[index];
                if (bytes >= unit)
                {
                    size = $"{bytes / unit:##.##} {k_ByteUnitsNames[index]}";
                    break;
                }
            }

            return size;
        }
    }
}
