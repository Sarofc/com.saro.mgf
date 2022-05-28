using Cysharp.Threading.Tasks;
using Saro.IO;
using System.IO;

namespace Saro.Core
{
    public static class IAssetInterfaceExtension
    {
        public static async UniTask<VFileSystem> OpenVFileSystemAsync(this IAssetManager self, string relativePath, FileMode mode, FileAccess access, int maxFileCount = 1024, int maxBlockCount = 1024)
        {
            var fullPath = await self.CheckRawBundlesAsync(relativePath);
            if (string.IsNullOrEmpty(fullPath)) return null;

            return VFileSystem.Open(fullPath, mode, access, maxFileCount, maxBlockCount);
        }
    }
}
