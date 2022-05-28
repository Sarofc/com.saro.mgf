using System;
using System.Collections.Generic;

namespace Saro
{
    public sealed class DefaultServiceLocator : IServiceLocator
    {
        private readonly Dictionary<Type, IService> m_ServiceMap;
        private Queue<Type> m_Updates, m_Updates2;

        public DefaultServiceLocator()
        {
            m_ServiceMap = new Dictionary<Type, IService>();
            m_Updates = new Queue<Type>();
            m_Updates2 = new Queue<Type>();
        }

        T IServiceLocator.Register<T>(T service)
        {
            var type = typeof(T);
            return Register(type, service) as T;
        }

        void IServiceLocator.Unregister<T>()
        {
            var type = typeof(T);
            Unregister(type);
        }

        T IServiceLocator.Register<T>()
        {
            var type = typeof(T);
            return Register(type) as T;
        }

        T IServiceLocator.Resolve<T>()
        {
            var type = typeof(T);
            return Resolve(type) as T;
        }

        public IService Register(Type type, IService service)
        {
            if (!m_ServiceMap.ContainsKey(type))
            {
                service.Awake();
                m_ServiceMap.Add(type, service);
                m_Updates.Enqueue(type);
                return service;
            }
            else
            {
                throw new ServiceLocatorException("duplicate  service type: " + type);
            }
        }

        public IService Register(Type type)
        {
            if (!m_ServiceMap.ContainsKey(type))
            {
                var service = Activator.CreateInstance(type) as IService;
                service.Awake();
                m_ServiceMap.Add(type, service);
                m_Updates.Enqueue(type);
                return service;
            }
            else
            {
                throw new ServiceLocatorException("duplicate  service type: " + type);
            }
        }

        public IService Resolve(Type type)
        {
            if (m_ServiceMap.TryGetValue(type, out var service))
            {
                return service as IService;
            }

            return null;
        }

        public void Unregister(Type type)
        {
            if (m_ServiceMap.TryGetValue(type, out var service))
            {
                service.Dispose();
            }

            m_ServiceMap.Remove(type);
        }

        public void Dispose()
        {
            foreach (var item in m_ServiceMap)
            {
                item.Value.Dispose();
            }

            m_ServiceMap.Clear();
        }

        public void Update()
        {
            while (m_Updates.Count > 0)
            {
                var type = m_Updates.Dequeue();
                if (!m_ServiceMap.TryGetValue(type, out var service))
                {
                    continue;
                }

                m_Updates2.Enqueue(type);

                service.Update();
            }

            Utility.RandomUtility.Swap(ref m_Updates, ref m_Updates2);
        }
    }
}