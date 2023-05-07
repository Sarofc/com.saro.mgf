using Cysharp.Threading.Tasks;
using System;

namespace Saro
{
    /*
     * TODO
     * 
     * 1. 加上依赖关系，类似之前的jobqueue，简化各个service之间的维护成本
     * 
     */

    public interface IServiceAwake
    {
        void Awake();
    }

    public interface IServiceUpdate
    {
        void Update();
    }

    public interface IService
    {
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
