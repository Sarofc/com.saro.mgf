#define USE_RECYCLABLE_MEMORY

using System.IO;
using Microsoft.IO;

namespace Saro.Net
{
    public static class MemoryStreamPool
    {
        // https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream
        private static readonly RecyclableMemoryStreamManager s_MemoryStreamManager = new();

        public static MemoryStream Rent(int count = 0)
        {
#if USE_RECYCLABLE_MEMORY
            return s_MemoryStreamManager.GetStream();
#else
            MemoryStream stream;
            if (count > 0)
            {
                stream = new MemoryStream(count);
            }
            else
            {
                stream = new MemoryStream();
            }

            return stream;
#endif
        }
    }
}