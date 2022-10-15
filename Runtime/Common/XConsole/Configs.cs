using System;
using System.Collections.Generic;

namespace Saro.XConsole
{
    [Serializable]
    public class Configs
    {
        public float width = 600, height = 350;
        public int logFlag = (int)LogStorage.ELogTypeFlag.All;
        public List<string> cmdHistories = new();
    }
}
