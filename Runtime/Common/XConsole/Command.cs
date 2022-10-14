#if true

using System;
using System.Reflection;

namespace Saro.XConsole
{
    internal class Command
    {
        private readonly MethodInfo m_Method;
        private readonly Type[] m_ParamsTypes;
        private readonly object m_Instance;
        private readonly string m_MethodSignature;

        public Command(MethodInfo method, Type[] paramsTypes, object instance, string methodSignature)
        {
            this.m_Method = method;
            this.m_ParamsTypes = paramsTypes;
            this.m_Instance = instance;
            this.m_MethodSignature = methodSignature;
        }

        public bool IsValid()
        {
            if (!m_Method.IsStatic && m_Instance.Equals(null))
            {
                return false;
            }
            return true;
        }

        public Type[] ParamsTypes => m_ParamsTypes;

        public bool IsParamsCountMatch(int count)
        {
            return m_ParamsTypes.Length == count;
        }

        public void Execute(object[] objects)
        {
            m_Method?.Invoke(m_Instance, objects);
        }

        /// <summary>
        /// complete info, include command and methodinfo
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_MethodSignature;
        }
    }
}

#endif
