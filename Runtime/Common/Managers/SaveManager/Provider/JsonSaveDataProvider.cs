using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Saro.Saving
{
    public sealed class JsonSaveDataProvider : ISaveDataProvider
    {
        private JsonSerializerSettings m_JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        void ISaveDataProvider.Load(ISaveFile saveFile)
        {
            try
            {
                var content = File.ReadAllText(saveFile.FilePath);
                var datas = JsonConvert.DeserializeObject<IList<ISaveData>>(content, m_JsonSerializerSettings);

                foreach (var item in datas)
                {
                    saveFile.AddSaveData(item);
                }
            }
            catch (Exception e)
            {
                Log.ERROR("[Save]", e);
            }
        }

        void ISaveDataProvider.Save(ISaveFile saveFile)
        {
            try
            {
                var content = JsonConvert.SerializeObject(saveFile.SaveDatas, m_JsonSerializerSettings);
                File.WriteAllText(saveFile.FilePath, content);
            }
            catch (Exception e)
            {
                Log.ERROR("[Save]", e);
            }
        }
    }
}
