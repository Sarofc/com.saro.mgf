#if UNITY_EDITOR

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Saro.Net.Http;
using UnityEngine;

namespace Saro.Net.Sample
{
    public class TestHttpService : MonoBehaviour
    {
        public readonly HttpService httpService = new();

        private void Awake()
        {
        }

        private void OnDestroy()
        {
            httpService?.Dispose();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestHttp();
            }
        }

        public async Task TestHttp()
        {
            var msg = "yahaha http!";
            var retJson = await httpService.PostAsync("http://localhost:8088", msg);
            Log.ERROR($"[http callback] {retJson}");
        }
    }
}

#endif