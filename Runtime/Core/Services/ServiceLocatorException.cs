using System;

namespace Saro
{
    public sealed class ServiceLocatorException : Exception
    {
        public ServiceLocatorException(string message) : base(message)
        {
        }
    }
}
