using System;
using System.Collections.Generic;
using NUnit.Framework;
using Saro.Collections;
using Saro.Utility;
using Assert = NUnit.Framework.Assert;

namespace Saro.MgfTests
{
    [TestFixture]
    class TestFNativeList
    {
        [TestCase(0)]
        [TestCase(10)]
        public void Test_AllocationSize(int capacity)
        {
            using FNativeList<int> list = new FNativeList<int>(capacity, EAllocator.Persistent);

            Assert.AreEqual(capacity, list.Capacity);
            Assert.AreEqual(0, list.Count);
        }

        [TestCase(0)]
        [TestCase(10)]
        public void Test_Resize(int capacity)
        {
            using FNativeList<int> list = new FNativeList<int>(0, EAllocator.Persistent);
            list.Resize(capacity);

            Assert.AreEqual(capacity, list.Capacity);
            Assert.AreEqual(0, list.Count);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void Test_Add(int capacity)
        {
            using FNativeList<int> list = new FNativeList<int>(capacity, EAllocator.Persistent);

            for (int i = 0; i < capacity; i++)
                list.Add(i);

            for (int i = 0; i < capacity; i++)
                Assert.AreEqual(i, list[i]);

            Assert.AreEqual(capacity, list.Capacity);
            Assert.AreEqual(capacity, list.Count);
        }

        [TestCase]
        public void Test_RemoveAt()
        {
            int capacity = 10;

            using FNativeList<int> list = new FNativeList<int>(capacity, EAllocator.Persistent);

            for (int i = 0; i < 10; i++)
                list.Add(i);

            list.RemoveAt(3);
            Assert.AreEqual(4, list[3]);

            list.RemoveAt(0);
            Assert.AreEqual(1, list[0]);

            list.RemoveAt(7);

            Assert.AreEqual(capacity, list.Capacity);
            Assert.AreEqual(7, list.Count);
        }

        [TestCase]
        public void Test_RemoveAtSwapBack()
        {
            int capacity = 10;

            using FNativeList<int> list = new FNativeList<int>(capacity, EAllocator.Persistent);

            for (int i = 0; i < 10; i++)
                list.Add(i);

            list.RemoveAtSwapBack(3);
            Assert.AreEqual(9, list[3]);

            list.RemoveAtSwapBack(0);
            Assert.AreEqual(8, list[0]);

            list.RemoveAtSwapBack(7);

            Assert.AreEqual(capacity, list.Capacity);
            Assert.AreEqual(7, list.Count);
        }

        [TestCase(5)]
        [TestCase(8)]
        [TestCase(10)]
        public void Test_Insert_End(int count)
        {
            int capacity = 20;

            using FNativeList<int> list = new(capacity, EAllocator.Persistent);
            List<int> listExpected = new(capacity);

            for (int i = 0; i < count; i++)
            {
                list.Insert(i, i);
                listExpected.Insert(i, i);

                Assert.AreEqual(listExpected[i], list[i]);
            }

            Assert.AreEqual(count, list.Count);
            Assert.AreEqual(capacity, list.Capacity);
        }

        [TestCase(5)]
        [TestCase(8)]
        [TestCase(10)]
        public void Test_Insert_Begin(int count)
        {
            int capacity = 20;

            using FNativeList<int> list = new(capacity, EAllocator.Persistent);
            List<int> listExpected = new(capacity);

            for (int i = 0; i < count; i++)
            {
                list.Insert(0, i);
                listExpected.Insert(0, i);

                Assert.AreEqual(listExpected[i], list[i]);
            }

            Assert.AreEqual(count, list.Count);
            Assert.AreEqual(capacity, list.Capacity);
        }

        [TestCase(1)]
        [TestCase(11)]
        [TestCase(33)]
        public void Test_AutoGrow(int size) // 自动扩容
        {
            using FNativeList<int> list = new(0, EAllocator.Persistent);

            for (int i = 0; i < size; i++)
            {
                list.Add(i);
                Assert.AreEqual(i, list[i]);
            }

            var defaultCapacity = FNativeList<int>.k_DefaultCapacity;
            var expected = defaultCapacity;
            while (size > expected)
            {
                expected *= 2;
            }

            Assert.AreEqual(expected, list.Capacity);
            Assert.AreEqual(size, list.Count);
        }

        [TestCase(1)]
        [TestCase(11)]
        [TestCase(33)]
        public void Test_AsSpan(int capacity)
        {
            using FNativeList<int> list = new FNativeList<int>(capacity, EAllocator.Persistent);

            for (int i = 0; i < capacity; i++)
                list.Add(i);

            var span = list.AsSpan();
            for (int i = 0; i < capacity; i++)
                Assert.AreEqual(i, span[i]);

            span[0] = 999;
            Assert.AreEqual(999, list[0]);

            Assert.AreEqual(capacity, list.Capacity);
            Assert.AreEqual(capacity, list.Count);
        }

        [TestCase(1)]
        [TestCase(6)]
        [TestCase(31)]
        public void Test_AsReadOnlySpan(int capacity)
        {
            using FNativeList<int> list = new FNativeList<int>(capacity, EAllocator.Persistent);

            for (int i = 0; i < capacity; i++)
                list.Add(i);

            var span = list.AsReadOnlySpan();
            for (int i = 0; i < capacity; i++)
                Assert.AreEqual(i, span[i]);

            Assert.AreEqual(capacity, list.Capacity);
            Assert.AreEqual(capacity, list.Count);
        }
    }
}