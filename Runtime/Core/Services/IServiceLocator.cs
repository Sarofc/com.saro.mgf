using Cysharp.Threading.Tasks;
using System;

namespace Saro
{
    public interface IService
    {
        void Awake();
        void Update();
        void Dispose();
    }

    public interface IServiceLocator
    {
        T Register<T>(T service) where T : class, IService;

        T Register<T>() where T : class, IService;

        T Resolve<T>() where T : class, IService;

        void Unregister<T>() where T : class, IService;

        IService Register(Type type, IService service);

        IService Register(Type type);

        IService Resolve(Type type);

        void Unregister(Type type);

        void Update();

        void Dispose();
    }
}
