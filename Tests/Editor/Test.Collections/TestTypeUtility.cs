using System.Collections.Generic;
using NUnit.Framework;
using Saro.Collections;
using System;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Saro.Utility;

namespace Saro.MgfTests
{
    public class TestTypeUtility
    {
        public static IEnumerable<Type> SourceTypes = new List<Type>
        {
            typeof(Foo),
            typeof(Foo1),
            typeof(Bar),
            typeof(Bar1),
            typeof(Value<Foo, Bar>),
        };

        internal class Foo
        {
            public object o;
        }

        internal class Foo1
        {
            public int i;
        }

        internal struct Bar
        {
            public int i;
        }

        internal struct Bar1
        {
            public object o;
        }

        internal struct Value<T1, T2>
        {
            public int x, y;
        }

        [Test]
        public void Type_IsUnmanaged(
            [ValueSource(nameof(SourceTypes))] Type type
            )
        {
            var expected = UnsafeUtility.IsUnmanaged(type);
            var actual = type.IsUnmanaged();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Utility_IsUnmanaged_Foo()
        {
            var expected = UnsafeUtility.IsUnmanaged<Foo>();
            var actual = NativeUtility.IsUnmanaged<Foo>();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Utility_IsUnmanaged_Foo1()
        {
            var expected = UnsafeUtility.IsUnmanaged<Foo1>();
            var actual = NativeUtility.IsUnmanaged<Foo1>();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Utility_IsUnmanaged_Bar()
        {
            var expected = UnsafeUtility.IsUnmanaged<Bar>();
            var actual = NativeUtility.IsUnmanaged<Bar>();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Utility_IsUnmanaged_Bar1()
        {
            var expected = UnsafeUtility.IsUnmanaged<Bar1>();
            var actual = NativeUtility.IsUnmanaged<Bar1>();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Utility_IsUnmanaged_Value()
        {
            var expected = UnsafeUtility.IsUnmanaged<Value<Bar, Foo>>();
            var actual = NativeUtility.IsUnmanaged<Value<Bar, Foo>>();

            Assert.AreEqual(expected, actual);
        }
    }
}