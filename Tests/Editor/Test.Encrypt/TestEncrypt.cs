using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestEncrypt
    {
        [Test]
        public void QuickXorBytes()
        {
            var str = "这是需要加密的文本";
            var key = "1234567";
            var bytes = Encoding.UTF8.GetBytes(str);
            var keyBytes = Encoding.UTF8.GetBytes(key);

            Saro.Utility.EncryptionUtility.QuickSelfXorBytes(bytes, keyBytes);

            var encrptText = Encoding.UTF8.GetString(bytes);
            Assert.IsTrue(str.CompareTo(encrptText) != 0, $"EncrptText: {encrptText}, Text: {str}");

            Saro.Utility.EncryptionUtility.QuickSelfXorBytes(bytes, keyBytes);
            var newText = Encoding.UTF8.GetString(bytes);

            Assert.IsTrue(str.CompareTo(newText) == 0, $"EncrptText: {encrptText}, NewText: {newText}");
        }

        [Test, Combinatorial]
        public void XorBytes(
            [Values(1, 10, 20, 30, 40)] int length,
            [Values("这是需要加密的文本", "这是需要加密的文本2")] string str,
            [Values("1234567", "12345")] string key
            )
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Saro.Utility.EncryptionUtility.SelfXorBytes(bytes, keyBytes, 0, Mathf.Min(length, keyBytes.Length));

            var encrptText = Encoding.UTF8.GetString(bytes);
            Assert.IsTrue(str.CompareTo(encrptText) != 0, $"EncrptText: {encrptText}, Text: {str}");

            Saro.Utility.EncryptionUtility.SelfXorBytes(bytes, keyBytes, 0, Mathf.Min(length, keyBytes.Length));
            var newText = Encoding.UTF8.GetString(bytes);

            Assert.IsTrue(str.CompareTo(newText) == 0, $"EncrptText: {encrptText}, NewText: {newText}");
        }
    }
}