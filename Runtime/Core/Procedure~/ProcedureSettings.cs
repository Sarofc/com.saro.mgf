using UnityEngine;
using System.Collections.Generic;

namespace Saro.Core
{
    [CreateAssetMenu(menuName = "Settings/ProcedureSettings", fileName = "ProcedureSettings")]
    public class ProcedureSettings : ScriptableObject
    {
#if UNITY_EDITOR
        public int selectedIndex = -1;
#endif
        public string start;
        public List<string> procedureList = new List<string>();
    }
}