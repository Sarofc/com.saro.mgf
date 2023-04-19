using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Saro.Utility;

namespace Saro.MgfTests
{
    public class TestListSpan
    {
        public static IEnumerable<List<int>> keyArray = new List<List<int>>
        {
            //new () {},
            new () { 1, 2, 3, 4},
            new () { 4,3,2,1},
            new () { 1,2,3,4,5,6,6,7,8,9,9,0,0,4,3,2,1,33},
            new () { 1,2,3,4,5556,7,8,3,0,0,4,3,2,1,33,31-1999},
        };


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_List_CreateSpan(
            [ValueSource(nameof(keyArray))] List<int> list
            )
        {
            var actual = list.ToList();
            CollectionsMarshalEx.CreateSpan(actual, 5);

            Assert.That(actual.Count, Is.EqualTo(5));

            var actualSpan = CollectionsMarshal.AsSpan(actual);
            for (int i = 0; i < actualSpan.Length; i++)
            {
                if (i < list.Count)
                    Assert.That(actualSpan[i], Is.EqualTo(list[i]));
                else
                    Assert.That(actualSpan[i], Is.EqualTo(0));
            }
        }
    }
}
