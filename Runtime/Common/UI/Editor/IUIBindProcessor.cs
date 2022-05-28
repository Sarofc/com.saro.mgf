#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace Saro.UI
{
    public interface IUIBindProcessor
    {
        Dictionary<string, Type> Binds { get; }
    }
}

#endif