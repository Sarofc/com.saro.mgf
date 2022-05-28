using System;
using System.Net;

namespace Saro.Net
{
    public enum EChannelType
    {
        Connect,
        Accept,
    }

    public abstract class AChannel : IDisposable
    {
        public long Id;
        
        public EChannelType ChannelType { get; protected set; }
        
        public int Error { get; set; }
        
        public IPEndPoint RemoteAddress { get; set; }

        protected bool IsDisposed => Id == 0;

        public abstract void Dispose();
    }
}