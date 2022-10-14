using System;
using Newtonsoft.Json;

namespace Saro.XConsole
{
    /// <summary>
    /// 保存持久化数据
    /// </summary>
    [Serializable]
    public class PersientConfigs
    {
        public const string k_Key = "Saro.XConsole.PersientConfigs";

        public float windowHeight, windowWidth;

        //public static bool TryLoad(out PersientConfigs settings)
        //{
        //    if (UnityEngine.PlayerPrefs.HasKey(k_Key))
        //    {
        //        settings = null;
        //        return false;
        //    }
        //    else
        //    {
        //        var json = UnityEngine.PlayerPrefs.GetString(k_Key);
        //        settings = JsonConvert.DeserializeObject<PersientConfigs>(json);
        //        return true;
        //    }
        //}
    }
}
