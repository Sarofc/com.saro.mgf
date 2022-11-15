using Saro.Utility;

namespace Saro
{
    [UnityEngine.Scripting.Preserve]
    internal class AotPreserve
    {
        void NewtonsoftJson()
        {
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<FKeyframe>();
        }
    }
}
