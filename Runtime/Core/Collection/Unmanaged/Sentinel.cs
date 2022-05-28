using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.Collections
{
    public struct Sentinel
    {
        public Sentinel(IntPtr ptr, uint readFlag)
        {
        }

        public TestThreadSafety TestThreadSafety()
        {
            return default;
        }

        public static uint readFlag { get; set; }
        public static uint writeFlag { get; set; }
    }

    public struct TestThreadSafety : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
