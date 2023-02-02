using NUnit.Framework;
using Saro.Utility;

namespace Saro.MgfTests.Hash
{
    public class TestMD5Hash
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void MD5_Hash(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var hash1 = HashUtility.GetMd5HexHash(input);
            var hash2 = HashUtility.___GetMd5HexHash(input);

            Assert.IsTrue(hash1 == hash2, $"ToHexString failed: {hash1} != {hash2}");
        }
    }
}
