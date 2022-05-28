using System;
using System.ComponentModel;

#if !SERVER
#endif

namespace Saro
{
    public class FObject : ISupportInitialize, IDisposable
    {
        public FObject()
        { }

        public virtual void BeginInit() { }

        public virtual void EndInit() { }

        public virtual void Dispose()
        { }

        public override string ToString()
        {
            // TODO 使用其他序列化库
            //return JsonUtility.ToJson(this);

            return base.ToString();
        }
    }
}
