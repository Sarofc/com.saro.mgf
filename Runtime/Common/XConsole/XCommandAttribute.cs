using System;

namespace Saro.XConsole
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class XCommandAttribute : Attribute
    {
        public readonly string cmd;
        public readonly string desc;

        public XCommandAttribute(string cmd, string desc)
        {
            this.cmd = cmd;
            this.desc = desc;
        }
    }
}
