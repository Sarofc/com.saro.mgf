using System.Collections.Generic;
using NUnit.Framework;
using Saro.Collections;
using System;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestTLinkedList
    {
        public TLinkedList<int> list;

        [SetUp]
        public void Setup()
        {
            list = new TLinkedList<int>();
        }

        [Test]
        public void AddFirst()
        {
            var node = new LinkedListNode<int>(1);
            list.AddFirst(node);
            Assert.IsTrue(list.First.Value == 1);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddFirst(2);
            Assert.IsTrue(list.First.Value == 2);
            Assert.IsTrue(list.CachedNodeCount == 0);

            var node2 = new LinkedListNode<int>(3);
            list.AddFirst(node2);
            Assert.IsTrue(list.First.Value == 3);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddFirst(1);
            Assert.IsTrue(list.First.Value == 1);
            Assert.IsTrue(list.CachedNodeCount == 0);
        }

        [Test]
        public void AddLast()
        {
            var node = new LinkedListNode<int>(1);
            list.AddLast(node);
            Assert.IsTrue(list.Last.Value == 1);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddLast(2);
            Assert.IsTrue(list.Last.Value == 2);
            Assert.IsTrue(list.CachedNodeCount == 0);

            var node2 = new LinkedListNode<int>(3);
            list.AddLast(node2);
            Assert.IsTrue(list.Last.Value == 3);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddLast(1);
            Assert.IsTrue(list.Last.Value == 1);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.Add(2);
            Assert.IsTrue(list.Last.Value == 2);
            Assert.IsTrue(list.CachedNodeCount == 0);
        }

        [Test]
        public void AddBefore()
        {
            var node = new LinkedListNode<int>(1);
            try
            {
                list.AddBefore(null, 1);
                Assert.Fail("Should Throw");
            }
            catch (System.Exception e)
            {
                Assert.IsTrue(e is ArgumentNullException);
            }
            list.Add(1);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddBefore(list.First, 2);
            Assert.IsTrue(list.First.Value == 2);
            Assert.IsTrue(list.CachedNodeCount == 0);

            var node2 = new LinkedListNode<int>(3);
            list.AddBefore(list.First, node2);
            Assert.IsTrue(list.First.Value == 3);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddBefore(list.Last, 1);
            Assert.IsTrue(list.Last.Previous.Value == 1);
            Assert.IsTrue(list.CachedNodeCount == 0);
        }

        [Test]
        public void AddAfter()
        {
            var node = new LinkedListNode<int>(1);
            try
            {
                list.AddAfter(null, 1);
                Assert.Fail("Should Throw");
            }
            catch (System.Exception e)
            {
                Assert.IsTrue(e is ArgumentNullException);
            }
            list.Add(1);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddAfter(list.First, 2);
            Assert.IsTrue(list.First.Next.Value == 2);
            Assert.IsTrue(list.CachedNodeCount == 0);

            var node2 = new LinkedListNode<int>(3);
            list.AddAfter(list.First, node2);
            Assert.IsTrue(list.First.Next.Value == 3);
            Assert.IsTrue(list.CachedNodeCount == 0);

            list.AddAfter(list.Last, 1);
            Assert.IsTrue(list.Last.Value == 1);
            Assert.IsTrue(list.CachedNodeCount == 0);
        }

        [Test]
        public void RemoveFirst()
        {
            try
            {
                list.RemoveFirst();
                Assert.Fail("Should Throw");
            }
            catch (System.Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);
            list.Add(6);
            list.Add(7);

            Assert.IsTrue(list.CachedNodeCount == 0);

            list.RemoveFirst();
            Assert.IsTrue(list.First.Value == 2);
            Assert.IsTrue(list.CachedNodeCount == 1);

            list.RemoveFirst();
            Assert.IsTrue(list.First.Value == 3);
            Assert.IsTrue(list.CachedNodeCount == 2);

            list.RemoveFirst();
            Assert.IsTrue(list.First.Value == 4);
            Assert.IsTrue(list.CachedNodeCount == 3);
        }

        [Test]
        public void RemoveLast()
        {
            try
            {
                list.RemoveLast();
                Assert.Fail("Should Throw");
            }
            catch (System.Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);
            list.Add(6);
            list.Add(7);

            Assert.IsTrue(list.CachedNodeCount == 0);

            list.RemoveLast();
            Assert.IsTrue(list.Last.Value == 6);
            Assert.IsTrue(list.CachedNodeCount == 1);

            list.RemoveLast();
            Assert.IsTrue(list.Last.Value == 5);
            Assert.IsTrue(list.CachedNodeCount == 2);

            list.RemoveLast();
            Assert.IsTrue(list.Last.Value == 4);
            Assert.IsTrue(list.CachedNodeCount == 3);
        }

        [Test]
        public void Remove()
        {
            var node = new LinkedListNode<int>(1);
            try
            {
                list.Remove(node);
                Assert.Fail("Should Throw");
            }
            catch (System.Exception)
            {
                // Assert.IsTrue(e is NullReferenceException);
            }

            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);
            list.Add(6);
            list.Add(7);

            Assert.IsTrue(list.CachedNodeCount == 0);

            list.Remove(3);
            var result = new int[] { 1, 2, 4, 5, 6, 7 };
            var index = 0;
            foreach (var item in list)
            {
                Assert.IsTrue(item == result[index++]);
            }
            Assert.IsTrue(list.CachedNodeCount == 1);

            list.Remove(list.First);
            Assert.IsTrue(list.First.Value == 2);
            Assert.IsTrue(list.CachedNodeCount == 2);

            list.Remove(list.Last);
            Assert.IsTrue(list.Last.Value == 6);
            Assert.IsTrue(list.CachedNodeCount == 3);
        }

        [Test]
        public void Clear()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);
            list.Add(6);
            list.Add(7);

            Assert.IsTrue(list.CachedNodeCount == 0);
            Assert.IsTrue(list.Count == 7);

            list.Clear();

            Assert.IsTrue(list.CachedNodeCount == 7);
            Assert.IsTrue(list.Count == 0);

            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(5);
            list.Add(6);
            list.Add(7);

            Assert.IsTrue(list.CachedNodeCount == 0);
            Assert.IsTrue(list.Count == 7);

            list.Clear();

            Assert.IsTrue(list.CachedNodeCount == 7);
            Assert.IsTrue(list.Count == 0);

            list.ClearCachedNodes();

            Assert.IsTrue(list.CachedNodeCount == 0);
        }
    }
}