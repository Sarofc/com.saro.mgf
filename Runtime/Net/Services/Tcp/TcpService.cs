using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Saro.Net
{
    /*
     * TODO 看看 System.IO.Pipeline
     */
    
    public sealed class TcpService : AService
    {
        private readonly Dictionary<long, TcpChannel> m_IDChannels = new();

        private readonly SocketAsyncEventArgs m_InnArgs = new();

        private Socket m_Acceptor;

        internal readonly HashSet<long> needStartSend = new();

        public TcpService(ThreadSynchronizationContext threadSynchronizationContext, ServiceType serviceType)
        {
            ServiceType = serviceType;
            base.threadSynchronizationContext = threadSynchronizationContext;
        }

        public TcpService(ThreadSynchronizationContext threadSynchronizationContext, EndPoint ipEndPoint, ServiceType serviceType)
        {
            ServiceType = serviceType;
            base.threadSynchronizationContext = threadSynchronizationContext;

            m_Acceptor = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_InnArgs.Completed += OnComplete;
            m_Acceptor.Bind(ipEndPoint);
            m_Acceptor.Listen(1000);

            Log.ERROR($"start socket. {ipEndPoint}");

            base.threadSynchronizationContext.PostNext(AcceptAsync);
        }

        private void OnComplete(object sender, SocketAsyncEventArgs e)
        {
            SocketError socketError = e.SocketError;
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    Socket acceptSocket = e.AcceptSocket;
                    threadSynchronizationContext.Post(() => { OnAcceptComplete(socketError, acceptSocket); });
                    break;
                default:
                    throw new($"socket error: {e.LastOperation}");
            }
        }

        #region 网络线程

        private void OnAcceptComplete(SocketError socketError, Socket acceptSocket)
        {
            if (m_Acceptor == null)
            {
                return;
            }

            // 开始新的accept
            AcceptAsync();

            if (socketError != SocketError.Success)
            {
                Log.ERROR($"accept error {socketError}");
                return;
            }

            try
            {
                long id = CreateAcceptChannelId(0);
                var channel = new TcpChannel(id, acceptSocket, this);
                m_IDChannels.Add(channel.Id, channel);
                long channelId = channel.Id;

                OnAccept(channelId, channel.RemoteAddress);
            }
            catch (Exception exception)
            {
                Log.ERROR(exception);
            }
        }


        private void AcceptAsync()
        {
            m_InnArgs.AcceptSocket = null;
            if (m_Acceptor.AcceptAsync(m_InnArgs))
            {
                return;
            }

            OnAcceptComplete(m_InnArgs.SocketError, m_InnArgs.AcceptSocket);
        }

        private TcpChannel Create(IPEndPoint ipEndPoint, long id)
        {
            TcpChannel channel = new(id, ipEndPoint, this);
            m_IDChannels.Add(channel.Id, channel);
            return channel;
        }

        protected override void Get(long id, IPEndPoint address)
        {
            if (m_IDChannels.TryGetValue(id, out TcpChannel _))
            {
                return;
            }

            Create(address, id);
        }

        private TcpChannel Get(long id)
        {
            m_IDChannels.TryGetValue(id, out TcpChannel channel);
            return channel;
        }

        public override void Dispose()
        {
            m_Acceptor?.Close();
            m_Acceptor = null;
            m_InnArgs.Dispose();
            threadSynchronizationContext = null;

            foreach (long id in m_IDChannels.Keys.ToArray())
            {
                TcpChannel channel = m_IDChannels[id];
                channel.Dispose();
            }

            m_IDChannels.Clear();
        }

        public override void Remove(long id)
        {
            if (m_IDChannels.TryGetValue(id, out TcpChannel channel))
            {
                channel.Dispose();
            }

            m_IDChannels.Remove(id);
        }

        protected override void Send(long channelId, long actorId, MemoryStream stream)
        {
            try
            {
                TcpChannel aChannel = Get(channelId);
                if (aChannel == null)
                {
                    OnError(channelId, ErrorCode.ERR_SendMessageNotFoundTChannel);
                    return;
                }

                aChannel.Send(actorId, stream);
            }
            catch (Exception e)
            {
                Log.ERROR(e);
            }
        }

        public override void Update()
        {
            foreach (long channelId in needStartSend)
            {
                TcpChannel tChannel = Get(channelId);
                tChannel?.Update();
            }

            needStartSend.Clear();
        }

        public override bool IsDispose()
        {
            return threadSynchronizationContext == null;
        }

        #endregion
    }
}