﻿using NUnit.Framework;
using Saro.Collections;
using System;

namespace Saro.MgfTests
{
    [TestFixture]
    public class TestDynamicArray
    {
        [Test]
        public void TestAlloc()
        {
            using (var _simpleNativeArray = NativeDynamicArray.Alloc<uint>(33))
            {

                Assert.That(_simpleNativeArray.Capacity<uint>(), Is.EqualTo(33));
            }
        }

        [Test]
        public void TestReAlloc()
        {
            using (var _simpleNativeArray = NativeDynamicArray.Alloc<uint>(33))
            {
                _simpleNativeArray.Resize<uint>(50);

                Assert.That(_simpleNativeArray.Capacity<uint>(), Is.EqualTo(50));
            }
        }

        [Test]
        public void TestAdd()
        {
            using (var _simpleNativeArray = NativeDynamicArray.Alloc<uint>())
            {
                for (var i = 0; i < 33; i++)
                    _simpleNativeArray.Add<uint>((uint)i);

                for (var i = 0; i < 33; i++)
                    Assert.That(_simpleNativeArray.Get<uint>(i), Is.EqualTo(i));

                Assert.That(_simpleNativeArray.Count<uint>(), Is.EqualTo(33));
            }
        }

        [Test]
        public void TestAddInBytes()
        {
            using (var _simpleNativeArray = NativeDynamicArray.Alloc<byte>())
            {
                for (var i = 0; i < 33; i++)
                    _simpleNativeArray.Add<byte>((byte)i);

                for (var i = 0; i < 33; i++)
                    Assert.That(_simpleNativeArray.Get<byte>(i), Is.EqualTo(i));

                Assert.That(_simpleNativeArray.Count<byte>(), Is.EqualTo(33));
            }
        }

        [Test]
        public void TestSetOutOfTheIndex()
        {
            using (var _simpleNativeArray = NativeDynamicArray.Alloc<uint>(20))
            {
                _simpleNativeArray.Set<uint>(10, 10);

                Assert.That(_simpleNativeArray.Get<uint>(10), Is.EqualTo(10));
                Assert.That(_simpleNativeArray.Count<uint>(), Is.EqualTo(11));
            }
        }

        [Test]
        public void TestSetOutOfTheCapacity()
        {
            using (var _simpleNativeArray = NativeDynamicArray.Alloc<uint>())
            {
                Assert.Throws<Exception>(() => _simpleNativeArray.Set<uint>(10, 10));
            }
        }
    }
}
