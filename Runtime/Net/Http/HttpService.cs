using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 
 TODO 测试，保证稳定
 
 */

namespace Saro.Net.Http
{
    public class HttpService : System.IDisposable, IService
    {
        private const int k_DefaultTimeout = 50000000; // 5s

        private readonly HttpClient m_HttpClient;

        public HttpService()
        {
            m_HttpClient = new();
            SetTimeout(k_DefaultTimeout);
        }

        public void SetTimeout(long timeout)
        {
            m_HttpClient.Timeout = new TimeSpan(timeout);
        }

        public async ValueTask<string> PostAsync(string url, string json, CancellationToken cancellationToken = default) //post异步请求方法
        {
            try
            {
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await m_HttpClient.PostAsync(url, content, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                Log.ERROR($"[▼ HTTP] PostAsync failed. StatusCode: {response.StatusCode}");
                return null;
            }
            catch (TaskCanceledException)
            {
                Log.INFO("PostAsync TaskCanceledException");
            }
            catch (Exception ex)
            {
                Log.ERROR(ex);
            }

            return null;
        }

        public async Task<Stream> PostAsync(string url, Stream stream, CancellationToken cancellationToken = default) //post异步请求方法
        {
            try
            {
                HttpContent content = new StreamContent(stream);
                HttpResponseMessage response = await m_HttpClient.PostAsync(url, content, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsStreamAsync();
                }

                Log.ERROR($"[▼ HTTP] PostAsync failed. StatusCode: {response.StatusCode}");

                return null;
            }
            catch (TaskCanceledException)
            {
                Log.INFO("PostAsync TaskCanceledException");
            }
            catch (Exception ex)
            {
                Log.ERROR(ex);
            }

            return null;
        }

        public void Dispose()
        {
            m_HttpClient?.Dispose();
        }
    }
}