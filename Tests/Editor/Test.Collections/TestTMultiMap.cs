using System.Collections.Generic;
using NUnit.Framework;
using Saro.Collections;
using System;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestTMultiMap
    {
        public TMultiMap<int, int> dic;

        [SetUp]
        public void Setup()
        {
            dic = new TMultiMap<int, int>();
        }

        [Test]
        public void Add()
        {
            dic.Add(1, 1);
            Assert.IsTrue(dic.Count == 1);
            Assert.IsTrue(dic[1].Count == 1);

            dic.Add(1, 1);
            Assert.IsTrue(dic.Count == 1);
            Assert.IsTrue(dic[1].Count == 2);

            dic.Add(2, 1);
            Assert.IsTrue(dic.Count == 2);
            Assert.IsTrue(dic[2].Count == 1);
        }

        [Test]
        public void Contains()
        {
            dic.Add(1, 1);
            dic.Add(1, 1);
            dic.Add(2, 1);
            dic.Add(2, 2);
            dic.Add(3, 3);

            Assert.IsTrue(dic.ContainsKey(1));
            Assert.IsTrue(dic.Contains(1, 1));

            Assert.IsFalse(dic.Contains(1, 2));
            Assert.IsFalse(dic.ContainsKey(0));
            Assert.IsFalse(dic.Contains(3, 2));
        }

        [Test]
        public void RemoveAll()
        {
            dic.Add(1, 1);
            dic.Add(1, 1);
            dic.Add(2, 1);
            dic.Add(2, 2);
            dic.Add(3, 3);

            Assert.IsTrue(dic.RemoveAll(1));
            Assert.IsFalse(dic.ContainsKey(1));
            Assert.IsFalse(dic.Contains(1, 1));
            Assert.IsTrue(dic.Count == 2);

            Assert.IsFalse(dic.RemoveAll(0));
            Assert.IsTrue(dic.Count == 2);

            Assert.IsTrue(dic.RemoveAll(3));
            Assert.IsFalse(dic.ContainsKey(3));
            Assert.IsFalse(dic.Contains(3, 2));
            Assert.IsTrue(dic.Count == 1);
        }

        [Test]
        public void Remove()
        {
            dic.Add(1, 1);
            dic.Add(1, 1);
            dic.Add(2, 1);
            dic.Add(2, 2);
            dic.Add(3, 3);

            Assert.IsTrue(dic.Remove(1, 1));//!!! Remove(key,value) only remove the first one
            Assert.IsTrue(dic.ContainsKey(1));
            Assert.IsTrue(dic.Contains(1, 1));
            Assert.IsTrue(dic.Count == 3);

            Assert.IsFalse(dic.Remove(0, 1));
            Assert.IsTrue(dic.Count == 3);

            Assert.IsFalse(dic.Remove(1, 0));
            Assert.IsTrue(dic.Count == 3);

            Assert.IsTrue(dic.Remove(2, 2));
            Assert.IsTrue(dic.Contains(2, 1));
            Assert.IsFalse(dic.Contains(2, 2));
            Assert.IsTrue(dic.Count == 3);
        }

        [Test]
        public void TryGetValue()
        {
            dic.Add(1, 1);
            dic.Add(1, 1);
            dic.Add(2, 1);
            dic.Add(2, 2);
            dic.Add(2, 4);
            dic.Add(3, 3);

            var b1 = dic.TryGetValue(1, out TLinkedListRange<int> range);
            Assert.IsTrue(b1);
            Assert.IsTrue(range.Count == 2);

            var b2 = dic.TryGetValue(2, out range);
            Assert.IsTrue(b2);
            Assert.IsTrue(range.Count == 3);

            var b3 = dic.TryGetValue(3, out range);
            Assert.IsTrue(b3);
            Assert.IsTrue(range.Count == 1);

            var b4 = dic.TryGetValue(0, out range);
            Assert.IsFalse(b4);
            Assert.IsTrue(range.Count == 0);
        }

        [Test]
        public void Clear()
        {
            dic.Add(1, 1);
            dic.Add(1, 1);
            dic.Add(2, 1);
            dic.Add(2, 2);
            dic.Add(2, 4);
            dic.Add(3, 3);

            Assert.IsTrue(dic.Count == 3);

            dic.Clear();

            Assert.IsTrue(dic.Count == 0);

            dic.Add(0, 1);
            dic.Add(1, 1);
            dic.Add(2, 1);
            dic.Add(2, 2);
            dic.Add(2, 4);
            dic.Add(3, 3);
            dic.Add(4, 1);

            Assert.IsTrue(dic.Count == 5);

            dic.Clear();

            Assert.IsTrue(dic.Count == 0);
        }

        [Test]
        public void Foreach()
        {
            var results = new List<(int, int)>
            {
                (1, 1),
                (1, 1),
                (2, 1),
                (2, 2),
                (2, 4),
                (3, 3),
            };

            foreach (var item in results)
            {
                dic.Add(item.Item1, item.Item2);
            }

            var index = 0;
            foreach (var item in dic)
            {
                var current = item.Value.Head;
                while (current != null && current != item.Value.Tail)
                {
                    //Debug.Log(item.Key + "," + current.Value);

                    Assert.IsTrue(item.Key == results[index].Item1 && current.Value == results[index].Item2, $"[{item.Key + "," + current.Value}] == [{results[index]}]");

                    current = current.Next;

                    index++;
                }
            }
        }
    }
}