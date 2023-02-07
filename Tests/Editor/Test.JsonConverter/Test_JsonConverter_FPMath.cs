#if FIXED_POINT_MATH

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

using ME.ECS.Mathematics;

namespace Saro.MgfTests.JsonConverter
{
    internal class Test_JsonConverter_FPMath
    {
        [Serializable]
        public class SerializeClass
        {
            public sfloat sfloat;
            public sfloat[] sfloat_array;

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
                sfloat = (sfloat)0.1f,
                sfloat_array = new[] { (sfloat)0.1f, (sfloat)0.2f, (sfloat)0.3f, (sfloat)0.4f, (sfloat)0.5f },

                float2 = new((sfloat)1.1f, (sfloat)2.2f),
                float2_array = new float2[] { new((sfloat)1.1f, (sfloat)2.2f), new((sfloat)0f, (sfloat)1f) },

                float3 = new((sfloat)1.1f, (sfloat)2.2f, (sfloat)3.3f),
                float3_list = new() { new((sfloat)1.1f, (sfloat)2.2f, (sfloat)3.3f), new((sfloat)0f, (sfloat)1f, (sfloat)0f) },

                float4 = new((sfloat)1.1f, (sfloat)2.2f, (sfloat)3.3f, (sfloat)4.4f),
                float4_list = new() { new((sfloat)1.1f, (sfloat)2.2f, (sfloat)3.3f, (sfloat)4.4f), new((sfloat)0f, (sfloat)1f, (sfloat)0f, (sfloat)1f) },

                quaternion = new((sfloat)1.1f, (sfloat)2.2f, (sfloat)3.3f, (sfloat)4.4f),
                quaternion_list = new() { new((sfloat)1.1f, (sfloat)2.2f, (sfloat)3.3f, (sfloat)4.4f), new((sfloat)0f, (sfloat)1f, (sfloat)0f, (sfloat)1f) },
            };

            var json = JsonHelper.ToJson(expectedSerializeClass);

            var actualSerializeClass = JsonHelper.FromJson<SerializeClass>(json);

            Assert.AreEqual(expectedSerializeClass.sfloat, actualSerializeClass.sfloat);
            Assert.AreEqual(expectedSerializeClass.float2, actualSerializeClass.float2);
            Assert.AreEqual(expectedSerializeClass.float3, actualSerializeClass.float3);
            Assert.AreEqual(expectedSerializeClass.float4, actualSerializeClass.float4);
            Assert.AreEqual(expectedSerializeClass.quaternion, actualSerializeClass.quaternion);

            Assert.AreEqual(expectedSerializeClass.sfloat_array, actualSerializeClass.sfloat_array);
            Assert.AreEqual(expectedSerializeClass.float2_array, actualSerializeClass.float2_array);
            Assert.AreEqual(expectedSerializeClass.float3_list, actualSerializeClass.float3_list);
            Assert.AreEqual(expectedSerializeClass.float4_list, actualSerializeClass.float4_list);
            Assert.AreEqual(expectedSerializeClass.quaternion_list, actualSerializeClass.quaternion_list);
        }
    }
}

#endif