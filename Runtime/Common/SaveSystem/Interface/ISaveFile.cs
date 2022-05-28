using System.Collections.Generic;

namespace Saro.SaveSystem
{
    public interface ISaveFile
    {
        string FilePath { get; set; }

        ISaveDataProvider SaveDataProvider { get; }

        IList<ISaveData> SaveDatas { get; }

        void AddSaveData(ISaveData saveData);

        void ClearSaveDatas();

        bool HasSaveFile();

        void Save();

        void Load();
    }
}