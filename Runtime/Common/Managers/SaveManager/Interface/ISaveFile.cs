using System.Collections.Generic;

namespace Saro.Saving
{
    public interface ISaveFile
    {
        /// <summary>
        /// 存档文件路径，可以是本地，也可以是远端，看子类实现
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// 存档数据加载接口，json、binary等等
        /// </summary>
        ISaveDataProvider SaveDataProvider { get; }

        /// <summary>
        /// 存档数据集合，一般一个模块实现一个 <see cref="ISaveData"/>
        /// </summary>
        IList<ISaveData> SaveDatas { get; }

        /// <summary>
        /// 添加 <see cref="ISaveData"/> 到 <see cref="SaveDatas"/>
        /// </summary>
        void AddSaveData(ISaveData saveData)
        {
            SaveDatas.Add(saveData);
        }

        /// <summary>
        /// 清理内存中的<see cref="SaveDatas"/>数据
        /// </summary>
        void ClearSaveDatas()
        {
            SaveDatas.Clear();
        }

        /// <summary>
        /// <see cref="FilePath"/> 路径是否存在存档文件，并未校验存档是否有效
        /// </summary>
        /// <returns></returns>
        bool HasSaveFile();

        void Save();

        void Load();
    }
}