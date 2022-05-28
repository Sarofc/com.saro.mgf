using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace Saro.Net.Sample
{
    public class TestTcpService : MonoBehaviour
    {
        private IPEndPoint ip;
        private Server server;
        private Client client;

        public void Start()
        {
            ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
            server = new Server();
            server.StartServer(ip);

            client = new Client();
            client.Connect(ip);

            client.Send("yahaha");
        }

        private void OnDestroy()
        {
            server.service.Dispose();
            client.service.Dispose();
        }

        private void Update()
        {
            server.service.Update();
            client.service.Update();
        }
    }

    public class Server
    {
        public AService service;

        public void StartServer(IPEndPoint ip)
        {
            service = new TcpService(ThreadSynchronizationContext.Current, ip, ServiceType.Outer);
            service.acceptCallback += OnAccept;
            service.errorCallback += OnError;
            service.readCallback += OnRead;
        }

        private void OnAccept(long channelId, IPEndPoint ip)
        {
            Debug.LogError($"OnAccept channelId:{channelId} ip:{ip}");
        }

        private void OnError(long channelId, int error)
        {
            Debug.LogError($"OnError ErrorCode: {error}");
        }

        private void OnRead(long channelId, MemoryStream ms)
        {
            var str = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
            Debug.LogError($"OnRead channelId: {channelId} ms: {str}");
        }
    }

    public class Client
    {
        public AService service;

        public long ChannelId { get; private set; }

        public void Connect(System.Net.IPEndPoint ip)
        {
            service = new TcpService(ThreadSynchronizationContext.Current, ServiceType.Outer);
            service.errorCallback += (id, e) => Log.ERROR($"{id} {e}");
            ChannelId = service.CreateConnectChannelId(0);
            service.GetOrCreate(ChannelId, ip);
        }

        public void Send(string msg)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(msg);
            using (var ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                service.SendStream(ChannelId, 0, ms);
            }
        }
    }
}