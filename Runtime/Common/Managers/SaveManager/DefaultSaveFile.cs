using System.Collections.Generic;
using System.IO;

namespace Saro.Saving
{
    public sealed class DefaultSaveFile : ISaveFile
    {
        public string FilePath { get; set; }

        public ISaveDataProvider SaveDataProvider { get; private set; }

        public IList<ISaveData> SaveDatas => m_SaveDatas;

        private List<ISaveData> m_SaveDatas;

        public DefaultSaveFile(string filePath, ISaveDataProvider saveDataProvider) : this(filePath, saveDataProvider, new List<ISaveData>())
        {
        }

        public DefaultSaveFile(string filePath, ISaveDataProvider saveDataProvider, List<ISaveData> saveDatas)
        {
            FilePath = filePath;
            SaveDataProvider = saveDataProvider;

            m_SaveDatas = saveDatas;
        }

        //public void AddSaveData(ISaveData saveData)
        //{
        //    m_SaveDatas.Add(saveData);
        //}

        //public void ClearSaveDatas()
        //{
        //    m_SaveDatas.Clear();
        //}

        public void Load()
        {
            ((ISaveFile)this).ClearSaveDatas();

            SaveDataProvider.Load(this);
        }

        public void Save()
        {
            SaveDataProvider.Save(this);
        }

        public bool HasSaveFile()
        {
            return File.Exists(FilePath);
        }
    }
}
