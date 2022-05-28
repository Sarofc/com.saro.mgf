using System;

namespace Saro.Events
{
    public sealed class MyEventException : Exception
    {
        public MyEventException(string message) : base(message)
        {
        }
    }
}
