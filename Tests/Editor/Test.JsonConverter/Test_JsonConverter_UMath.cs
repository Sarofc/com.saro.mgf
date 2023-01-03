using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Saro.Utility;
using UnityEditor;
using UnityEngine;

using Unity.Mathematics;

namespace Saro.MgfTests.JsonConverter
{
    internal class Test_JsonConverter_UMath
    {
        [Serializable]
        public class SerializeClass
        {
            public float2 float2;
            public float2[] float2_array;

            public float3 float3;
            public List<float3> float3_list;

            public float4 float4;
            public List<float4> float4_list;

            public quaternion quaternion;
            public List<quaternion> quaternion_list;

        }

        //[SetUp]
        //public void Setup()
        //{
        //}

        //[TearDown]
        //public void TearDown()
        //{
        //}

        [Test]
        public void Check()
        {
            var expectedSerializeClass = new SerializeClass()
            {
                float2 = new(1.1f, 2.2f),
                float2_array = new float2[] { new(1.1f, 2.2f), new(0f, 1f) },

                float3 = new(1.1f, 2.2f, 3.3f),
                float3_list = new() { new(1.1f, 2.2f, 3.3f), new(0f, 1f, 0f) },

                float4 = new(1.1f, 2.2f, 3.3f, 4.4f),
                float4_list = new() { new(1.1f, 2.2f, 3.3f, 4.4f), new(0f, 1f, 0f, 1f) },

                quaternion = new(1.1f, 2.2f, 3.3f, 4.4f),
                quaternion_list = new() { new(1.1f, 2.2f, 3.3f, 4.4f), new(0f, 1f, 0f, 1f) },
            };

            var json = JsonHelper.ToJson(expectedSerializeClass);

            var actualSerializeClass = JsonHelper.FromJson<SerializeClass>(json);

            Assert.AreEqual(expectedSerializeClass.float2, actualSerializeClass.float2);
            Assert.AreEqual(expectedSerializeClass.float3, actualSerializeClass.float3);
            Assert.AreEqual(expectedSerializeClass.float4, actualSerializeClass.float4);
            Assert.AreEqual(expectedSerializeClass.quaternion, actualSerializeClass.quaternion);

            Assert.AreEqual(expectedSerializeClass.float2_array, actualSerializeClass.float2_array);
            Assert.AreEqual(expectedSerializeClass.float3_list, actualSerializeClass.float3_list);
            Assert.AreEqual(expectedSerializeClass.float4_list, actualSerializeClass.float4_list);
            Assert.AreEqual(expectedSerializeClass.quaternion_list, actualSerializeClass.quaternion_list);
        }
    }
}
