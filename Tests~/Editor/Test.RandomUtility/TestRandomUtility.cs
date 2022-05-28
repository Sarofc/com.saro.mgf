using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestRandomUtility
    {
        // public const int count = 54;
        [System.NonSerialized] public List<int> list;
        [System.NonSerialized] public int[] array;

        private int sum;

        [SetUp]
        public void Start()
        {
            Saro.Utility.RandomUtility.InitSeed((int)DateTime.Now.Ticks);
        }

        [TestCase(2)]
        [TestCase(10)]
        [TestCase(66)]
        public void ShuffleArray(int count)
        {
            sum = 0;
            array = new int[count];

            for (int i = 1; i <= count; i++)
            {
                array[i - 1] = i;

                sum += i;
            }


            Saro.Utility.RandomUtility.Shuffle(array);

            var counter = 0;
            //bool result = false;
            for (int i = 0; i < array.Length; i++)
            {
                counter += array[i];
                //if (array[i] != i + 1) result = true;
            }

            //Debug.Log($"array {count} : {string.Join(",", array)}");

            Assert.IsTrue(counter == sum, $"counter: {counter}, sum: {sum}");
            // Assert.IsTrue(result);
        }

        [TestCase(2)]
        [TestCase(10)]
        [TestCase(66)]
        public void ShuffleList(int count)
        {
            sum = 0;
            list = new List<int>(count);

            for (int i = 1; i <= count; i++)
            {
                list.Add(i);

                sum += i;
            }

            Saro.Utility.RandomUtility.Shuffle(list);

            var counter = 0;
            //bool result = false;
            for (int i = 0; i < list.Count; i++)
            {
                counter += list[i];
                //if (list[i] != i + 1) result = true;
            }

            //Debug.Log($"list {count} : {string.Join(",", list)}");

            Assert.IsTrue(counter == sum, $"counter: {counter}, sum: {sum}");
            // Assert.IsTrue(result);
        }

        [TestCase(2)]
        [TestCase(10)]
        [TestCase(66)]
        public void SameSeed(int count)
        {
            sum = 0;
            list = new List<int>(count);

            for (int i = 1; i <= count; i++)
            {
                list.Add(i);

                sum += i;
            }

            Saro.Utility.RandomUtility.InitSeed(100);
            var clone = list.ToArray();
            Saro.Utility.RandomUtility.Shuffle(clone);

            Saro.Utility.RandomUtility.InitSeed(100);
            var clone2 = list.ToArray();
            Saro.Utility.RandomUtility.Shuffle(clone2);

            bool result = true;
            for (int i = 0; i < clone.Length; i++)
            {
                if (clone[i] != clone2[i])
                {
                    result = false;
                    break;
                }
            }
            Assert.IsTrue(result);
        }

        [TestCase(2)]
        [TestCase(10)]
        [TestCase(66)]
        public void DiffSeed(int count)
        {
            sum = 0;
            array = new int[count];

            for (int i = 1; i <= count; i++)
            {
                array[i - 1] = i;

                sum += i;
            }

            Saro.Utility.RandomUtility.InitSeed(100);
            var clone = new int[array.Length];
            array.CopyTo(clone, 0);
            Saro.Utility.RandomUtility.Shuffle(clone);

            var clone2 = new int[array.Length];
            array.CopyTo(clone2, 0);
            Saro.Utility.RandomUtility.Shuffle(clone2);

            bool result = true;
            for (int i = 0; i < clone.Length; i++)
            {
                if (clone[i] != clone2[i])
                {
                    result = false;
                    break;
                }
            }
            Assert.IsFalse(result);
        }
    }
}