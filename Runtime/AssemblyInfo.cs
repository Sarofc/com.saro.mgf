using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Saro.MGF.Tests")]
[assembly: InternalsVisibleTo("Saro.Gameplay")]

#if !ODIN_INSPECTOR
namespace Sirenix.OdinInspector
{
    public class ReadOnlyAttribute : Attribute { }
    public class InlineEditorAttribute : Attribute { }
    public class ButtonAttribute : Attribute { }
}
#endif
