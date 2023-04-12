using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using Saro.Utility;
//using Unity.PerformanceTesting;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestArrayUtility_Sort
    {
        public static IEnumerable<List<int>> keyArray = new List<List<int>>
        {
            //new () {},
            new () { 1, 2, 3, 4},
            new () { 4,3,2,1},
            new () { 1,2,3,4,5,6,6,7,8,9,9,0,0,4,3,2,1,33},
            new () { 1,2,3,4,5556,7,8,3,0,0,4,3,2,1,33,31-1999},
        };

        public struct Value : IComparable<Value>
        {
            public int value;
            public int CompareTo(Value other)
            {
                return Comparer<int>.Default.Compare(value, other.value);
            }
        }

        [Test]
        public void Test_ArraySort_T([ValueSource(nameof(keyArray))] List<int> src)
        {
            var expected = src.ToArray();
            Array.Sort(expected);

            var actual = src.ToArray();
            ArrayNonAlloc.Sort(actual, 0, actual.Length);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Test_ArraySort_T_Comparer([ValueSource(nameof(keyArray))] List<int> src)
        {
            var cast = MemoryMarshal.Cast<int, Value>(src.ToArray().AsSpan());

            var expected = cast.ToArray();
            Array.Sort(expected);

            var actual = cast.ToArray();
            ArrayNonAlloc.Sort(actual);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Test_ListSort_T(
            [ValueSource(nameof(keyArray))] List<int> array
            )
        {
            var expected = array.ToList();
            expected.Sort();

            var actual = array.ToList();
            ArrayNonAlloc.Sort(actual);
        }
    }
}