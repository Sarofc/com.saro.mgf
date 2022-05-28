using System;
using System.IO;
using System.Net;

namespace Saro.Net
{
    public enum ServiceType
    {
        Outer,
        Inner,
    }

    public abstract class AService : IDisposable
    {
        public ServiceType ServiceType { get; protected set; }

        public ThreadSynchronizationContext threadSynchronizationContext;

        // localConn放在低32bit
        private long m_ConnectIdGenerator = int.MaxValue;

        public long CreateConnectChannelId(uint localConn)
        {
            return (--m_ConnectIdGenerator << 32) | localConn;
        }

        public uint CreateRandomLocalConn()
        {
            return (1u << 30) | RandomHelper.RandUInt32();
        }

        // localConn放在低32bit
        private long m_AcceptIdGenerator = 1;

        public long CreateAcceptChannelId(uint localConn)
        {
            return (++m_AcceptIdGenerator << 32) | localConn;
        }

        public abstract void Update();

        public abstract void Remove(long id);

        public abstract bool IsDispose();

        protected abstract void Get(long id, IPEndPoint address);

        public abstract void Dispose();

        protected abstract void Send(long channelId, long actorId, MemoryStream stream);

        protected void OnAccept(long channelId, IPEndPoint ipEndPoint)
        {
            acceptCallback.Invoke(channelId, ipEndPoint);
        }

        public void OnRead(long channelId, MemoryStream memoryStream)
        {
            readCallback.Invoke(channelId, memoryStream);
        }

        public void OnError(long channelId, int e)
        {
            Remove(channelId);

            errorCallback?.Invoke(channelId, e);
        }


        public Action<long, IPEndPoint> acceptCallback;
        public Action<long, int> errorCallback;
        public Action<long, MemoryStream> readCallback;

        public void RemoveChannel(long channelId)
        {
            Remove(channelId);
        }

        public void SendStream(long channelId, long actorId, MemoryStream stream)
        {
            Send(channelId, actorId, stream);
        }

        public void GetOrCreate(long id, IPEndPoint address)
        {
            Get(id, address);
        }
    }
}