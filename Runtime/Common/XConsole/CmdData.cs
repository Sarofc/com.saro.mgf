using System;
using System.Collections.Generic;
using System.Reflection;

namespace Saro.XConsole
{
    public class CmdData
    {
        private readonly MethodInfo m_Method;
        private readonly Type[] m_ParamsTypes;
        private readonly object m_Instance;
        private readonly string m_MethodSignature;

        public CmdData(MethodInfo method, Type[] paramsTypes, object instance, string methodSignature)
        {
            m_Method = method;
            m_ParamsTypes = paramsTypes;
            m_Instance = instance;
            m_MethodSignature = methodSignature;
        }

        public bool IsValid()
        {
            if (!m_Method.IsStatic && m_Instance.Equals(null))
            {
                return false;
            }
            return true;
        }

        public IReadOnlyList<Type> ParamsTypes => m_ParamsTypes;

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
