using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Saro.IO.Test
{
    public class TestBinaryExtension : MonoBehaviour
    {
        [Serializable]
        public struct TestStruct
        {
            public int a;
            public float b;
            public long c;
        }

        static IEnumerable<int[]> source_ArrayInt = new List<int[]>
        {
            new int[] { 1, 2, 3, 4},
            new int[] { 4,3,2,1},
            new int[] { 1,2,3,4,5,6,6,7,8,9,9,0,0,4,3,2,1,33},
            new int[] { 1,2,3,4,5556,7,8,3,0,0,4,3,2,1,33,31-1999},
        };


        static IEnumerable<TestStruct[]> source_ArrayStruct = new List<TestStruct[]>
        {
            new TestStruct[] {
                new TestStruct { a = 1, b = 1.1f, c = 1000u },
                new TestStruct { a = 2, b = 2.2f, c = 2000u },
                new TestStruct { a = 3, b = 3.3f, c = 3000u },
                new TestStruct { a = 4, b = 4.4f, c = 4000u },
            },
        };

        static IEnumerable<TestStruct> source_Struct = new List<TestStruct>
        {
            new TestStruct { a = 1, b = 1.1f, c = 1000u },
            new TestStruct { a = 2, b = 2.2f, c = 2000u },
            new TestStruct { a = 3, b = 3.3f, c = 3000u },
            new TestStruct { a = 4, b = 4.4f, c = 4000u },
        };


        static IEnumerable<int> source_Int = new List<int> { 1, 2, 3, 4 };

        [Test]
        public void SerializeDeserialize_ArrayInt([ValueSource(nameof(source_ArrayInt))] int[] input)
        {
            var actual = input.ToArray();

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.WriteArrayUnmanaged(ref actual, actual.Length);

            ms.Position = 0;
            using var reader = new BinaryReader(ms);
            var actualCount = reader.ReadArrayUnmanaged(ref actual);

            Assert.AreEqual(input.Length, actualCount);
            Assert.AreEqual(input, actual);
        }

        [Test]
        public void SerializeDeserialize_ArrayStruct([ValueSource(nameof(source_ArrayStruct))] TestStruct[] input)
        {
            var actual = input.ToArray();

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.WriteArrayUnmanaged(ref actual, actual.Length);

            ms.Position = 0;
            using var reader = new BinaryReader(ms);
            var actualCount = reader.ReadArrayUnmanaged(ref actual);

            Assert.AreEqual(input.Length, actualCount);
            Assert.AreEqual(input, actual);
        }

        [Test]
        public void SerializeDeserialize_Struct([ValueSource(nameof(source_Struct))] TestStruct input)
        {
            var actual = input; // copy

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.WriteUnmanaged(ref actual);

            ms.Position = 0;
            using var reader = new BinaryReader(ms);
            reader.ReadUnmanaged(ref actual);

            Assert.AreEqual(input, actual);
        }

        [Test]
        public void SerializeDeserialize_Int([ValueSource(nameof(source_Int))] int input)
        {
            var actual = input; // copy

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.WriteUnmanaged(ref actual);

            ms.Position = 0;
            using var reader = new BinaryReader(ms);
            reader.ReadUnmanaged(ref actual);

            Assert.AreEqual(input, actual);
        }
    }
}
