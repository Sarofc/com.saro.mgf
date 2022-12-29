using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Saro.Utility;
using UnityEditor;
using UnityEngine;

namespace Saro.MgfTests.JsonConverter
{
    internal class Test_JsonConverter_UObjectToGUIDConverter
    {
        [Serializable]
        public class SerializeClass
        {
            [JsonConverter(typeof(UObjectToGUIDConverter))]
            public FooSO foo;
            [JsonConverter(typeof(UObjectToGUIDConverter))]
            public List<ScriptableObject> so_list;
            [JsonConverter(typeof(UObjectToGUIDConverter))]
            public ScriptableObject[] so_array;
        }

        string fooPath = "Packages\\com.saro.mgf\\Tests\\Editor\\Test.JsonConverter\\foo.asset";
        string barPath = "Packages\\com.saro.mgf\\Tests\\Editor\\Test.JsonConverter\\bar.asset";

        [SetUp]
        public void Setup()
        {
            var foo = ScriptableObject.CreateInstance<FooSO>();
            var bar = ScriptableObject.CreateInstance<BarSO>();

            AssetDatabase.CreateAsset(foo, fooPath);
            AssetDatabase.CreateAsset(bar, barPath);

            AssetDatabase.ImportAsset(fooPath);
            AssetDatabase.ImportAsset(barPath);

            //AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(fooPath);
            AssetDatabase.DeleteAsset(barPath);

            //AssetDatabase.Refresh();
        }

        [Test]
        public void Check()
        {
            var foo = AssetDatabase.LoadAssetAtPath<FooSO>(fooPath);
            var bar = AssetDatabase.LoadAssetAtPath<BarSO>(barPath);

            var expectedSerializeClass = new SerializeClass()
            {
                foo = foo,
                so_list = new() { foo, bar, },
                so_array = new ScriptableObject[] { foo, bar },
            };

            var json = JsonHelper.ToJson(expectedSerializeClass);

            var actualSerializeClass = JsonHelper.FromJson<SerializeClass>(json);

            Assert.AreEqual(expectedSerializeClass.foo, actualSerializeClass.foo);

            Assert.AreEqual(expectedSerializeClass.so_list.Count, actualSerializeClass.so_list.Count);
            Assert.AreEqual(expectedSerializeClass.so_list[0], actualSerializeClass.so_list[0]);
            Assert.AreEqual(expectedSerializeClass.so_list[1], actualSerializeClass.so_list[1]);

            Assert.AreEqual(expectedSerializeClass.so_array.Length, actualSerializeClass.so_array.Length);
            Assert.AreEqual(expectedSerializeClass.so_array[0], actualSerializeClass.so_array[0]);
            Assert.AreEqual(expectedSerializeClass.so_array[1], actualSerializeClass.so_array[1]);
        }
    }
}
