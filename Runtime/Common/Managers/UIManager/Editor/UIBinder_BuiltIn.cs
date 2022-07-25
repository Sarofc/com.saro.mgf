#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Saro.UI
{
    internal class UIBinder_BuiltIn : IUIBindProcessor
    {
        public Dictionary<string, Type> Binds { get; } = new Dictionary<string, Type>
        {
            {"txt_",typeof(Text)},
            {"img_",typeof(Image)},
            {"rawimg_",typeof(RawImage)},
            {"btn_",typeof(Button)},
            {"scrollbar_",typeof(Scrollbar)},
            {"scrollrect_",typeof(ScrollRect)},
            {"input_",typeof(InputField)},
            {"drop_",typeof(Dropdown)},
            {"slider_",typeof(Slider)},
            {"toggle_",typeof(Toggle)},
            {"togglegroup_",typeof(ToggleGroup)},
            {"go_",typeof(GameObject)},
            {"bin_",typeof(UIBinder)},
        };
    }
}

#endif