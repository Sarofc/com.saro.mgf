using NUnit.Framework;
using Saro.Collections;
using Saro.Utility;
using Assert = NUnit.Framework.Assert;

namespace Saro.MgfTests
{
    [TestFixture]
    class TestDynamicArrayCast
    {
        [TestCase]
        public void TestAllocationSize0()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, EAllocator.Persistent);

            Assert.That(fasterList.Capacity, Is.EqualTo(0));
            Assert.That(fasterList.Count, Is.EqualTo(0));

            fasterList.Dispose();
        }

        [TestCase]
        public void TestAllocationSize1()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(1, EAllocator.Persistent);

            Assert.That(fasterList.Capacity, Is.EqualTo(1));
            Assert.That(fasterList.Count, Is.EqualTo(0));

            fasterList.Dispose();
        }

        [TestCase]
        public void TestResize()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, EAllocator.Persistent);

            fasterList.Resize(10);

            Assert.That(fasterList.Capacity, Is.EqualTo(10));
            Assert.That(fasterList.Count, Is.EqualTo(0));

            fasterList.Dispose();
        }

        [TestCase]
        public void TestResizeTo0()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(10, EAllocator.Persistent);

            fasterList.Resize(0);

            Assert.That(fasterList.Capacity, Is.EqualTo(0));
            Assert.That(fasterList.Count, Is.EqualTo(0));

            fasterList.Dispose();
        }

        [TestCase]
        public void TestAdd()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, EAllocator.Persistent);

            for (int i = 0; i < 10; i++)
                fasterList.Add(i);

            for (int i = 0; i < 10; i++)
                Assert.That(fasterList[i], Is.EqualTo(i));

            Assert.That(fasterList.Capacity, Is.EqualTo(10));
            Assert.That(fasterList.Count, Is.EqualTo(10));

            fasterList.Dispose();
        }

        [TestCase]
        public void TestRemoveAt()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, EAllocator.Persistent);

            for (int i = 0; i < 10; i++)
                fasterList.Add(i);

            fasterList.RemoveAt(3);
            Assert.That(fasterList[3], Is.EqualTo(4));

            fasterList.RemoveAt(0);
            Assert.That(fasterList[0], Is.EqualTo(1));

            fasterList.RemoveAt(7);

            Assert.That(fasterList.Capacity, Is.EqualTo(10));
            Assert.That(fasterList.Count, Is.EqualTo(7));

            fasterList.Dispose();
        }

        [TestCase]
        public void TestUnorderedRemoveAt()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, EAllocator.Persistent);

            for (int i = 0; i < 10; i++)
                fasterList.Add(i);

            fasterList.UnorderedRemoveAt(3);
            Assert.That(fasterList[3], Is.EqualTo(9));

            fasterList.UnorderedRemoveAt(0);
            Assert.That(fasterList[0], Is.EqualTo(8));

            fasterList.UnorderedRemoveAt(7);

            Assert.That(fasterList.Capacity, Is.EqualTo(10));
            Assert.That(fasterList.Count, Is.EqualTo(7));

            fasterList.Dispose();
        }

        [TestCase]
        public void TesSetAt()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, EAllocator.Persistent);

            fasterList.AddAt(10) = 10;

            Assert.That(fasterList[10], Is.EqualTo(10));

            Assert.That(fasterList.Capacity, Is.EqualTo(16));
            Assert.That(fasterList.Count, Is.EqualTo(11));

            fasterList.Dispose();
        }
    }
}