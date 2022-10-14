#if true

using System;

namespace Saro.XConsole
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public readonly string command;

        public readonly string description;

        public CommandAttribute(string command, string description)
        {
            this.command = command;
            this.description = description;
        }
    }
}

#endif