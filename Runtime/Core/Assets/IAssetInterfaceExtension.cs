using Cysharp.Threading.Tasks;
using Saro.IO;
using System.IO;

namespace Saro.Core
{
    public static class IAssetInterfaceExtension
    {
        public static byte[] ReadVFile(this IAssetManager self, string relativePath, string subFile, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, int maxFileCount = 1024, int maxBlockCount = 1024)
        {
            var fullPath = self.GetRawFilePath(relativePath);
            if (string.IsNullOrEmpty(fullPath)) return null;
            using (var vfile = VFileSystem.Open(fullPath, mode, access, maxFileCount, maxBlockCount))
                return vfile.ReadFile(subFile);
        }

        public static async UniTask<byte[]> ReadVFileAsync(this IAssetManager self, string relativePath, string subFile, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, int maxFileCount = 1024, int maxBlockCount = 1024)
        {
            var fullPath = await self.GetRawFilePathAsync(relativePath);
            if (string.IsNullOrEmpty(fullPath)) return null;
            using (var vfile = VFileSystem.Open(fullPath, mode, access, maxFileCount, maxBlockCount))
                return vfile.ReadFile(subFile);
        }

        public static VFileSystem OpenVFile(this IAssetManager self, string relativePath, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, int maxFileCount = 1024, int maxBlockCount = 1024)
        {
            var fullPath = self.GetRawFilePath(relativePath);
            if (string.IsNullOrEmpty(fullPath)) return null;
            return VFileSystem.Open(fullPath, mode, access, maxFileCount, maxBlockCount);
        }

        public static async UniTask<VFileSystem> OpenVFileAsync(this IAssetManager self, string relativePath, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, int maxFileCount = 1024, int maxBlockCount = 1024)
        {
            var fullPath = await self.GetRawFilePathAsync(relativePath);
            if (string.IsNullOrEmpty(fullPath)) return null;
            return VFileSystem.Open(fullPath, mode, access, maxFileCount, maxBlockCount);
        }
    }
}
