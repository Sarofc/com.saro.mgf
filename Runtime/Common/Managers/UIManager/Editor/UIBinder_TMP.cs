#if UNITY_EDITOR && PACKAGE_TMP

namespace Saro.UI
{
    using TMPro;
    using System;
    using System.Collections.Generic;

    internal class UIBinder_TMP : IUIBindProcessor
    {
        public Dictionary<string, Type> Binds { get; } = new Dictionary<string, Type>
        {
            {"tmptxt_",typeof(TMP_Text)},
            {"tmpinput_",typeof(TMP_InputField)},
        };
    }
}

#endif
