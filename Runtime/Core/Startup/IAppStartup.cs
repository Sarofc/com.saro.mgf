using Cysharp.Threading.Tasks;

namespace Saro
{
    /// <summary>
    /// 实现此接口，会被 <see cref="Main"/> 自动反射调用
    /// </summary>
    public interface IAppStartup
    {
        UniTask StartAsync();
    }
}