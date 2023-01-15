using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

using sfloat3 = ME.ECS.Mathematics.float3;

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

        static IEnumerable<sfloat3> source_sfloat3 = new List<sfloat3>
        {
            new sfloat3 (sfloat.FromRaw(1),sfloat.FromRaw(1),sfloat.FromRaw(1)),
            new sfloat3 (sfloat.FromRaw(11),sfloat.FromRaw(12),sfloat.FromRaw(13)),
            new sfloat3 (sfloat.FromRaw(122),sfloat.FromRaw(1),sfloat.FromRaw(1)),
            new sfloat3 (sfloat.FromRaw(333),sfloat.FromRaw(444),sfloat.FromRaw(555)),
        };

        static IEnumerable<(float3, bool)> source_float3_bool = new List<(float3, bool)>
        {
            (new float3 (1,2,3),                       true),
            (new float3 (11,22,33),                  false),
            (new float3 (1111,222,3333),        true),
            (new float3 (12,23,34),                 false),
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
        public void SerializeDeserialize_sfloat3([ValueSource(nameof(source_sfloat3))] sfloat3 input)
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
        public void SerializeDeserialize_float3_bool([ValueSource(nameof(source_float3_bool))] (float3, bool) input)
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

        [Test]
        public void SerializeDeserialize_UseIndex_ArrayStruct([ValueSource(nameof(source_ArrayStruct))] TestStruct[] input)
        {
            var actual = input.ToArray();

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.WriteArrayUnmanaged(ref actual, actual.Length - 1, 1);

            ms.Position = 0;
            using var reader = new BinaryReader(ms);
            var actualCount = reader.ReadArrayUnmanaged(ref actual, 1) + 1;

            Assert.AreEqual(input.Length, actualCount);
            Assert.AreEqual(input, actual);
        }
    }
}
