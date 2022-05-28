#pragma warning disable CS1591
#pragma warning disable CS0436

using Cysharp.Threading.Tasks.CompilerServices;
using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks
{
    [AsyncMethodBuilder(typeof(AsyncUniTaskVoidMethodBuilder))]
    public readonly struct UniTaskVoid
    {
        public void Forget()
        {
        }
    }
}

