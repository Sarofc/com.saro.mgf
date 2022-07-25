using System;
using System.Collections.Generic;
using System.IO;

namespace Saro.SaveSystem
{
    public sealed class BinarySaveDataProvider : ISaveDataProvider
    {
        void ISaveDataProvider.Load(ISaveFile saveFile)
        {
            bool error = false;
            try
            {
                using (var fs = new FileStream(saveFile.FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        var saveDataNum = br.ReadInt32();

                        var saveDatas = new List<ISaveData>(saveDataNum);
                        for (int i = 0; i < saveDataNum; i++)
                        {
                            var typeString = br.ReadString();

                            var type = Type.GetType(typeString);
                            var saveData = Activator.CreateInstance(type) as IBinarySaveData;

                            saveData.LoadFields(br);

                            saveDatas.Add(saveData);
                        }

                        for (int i = 0; i < saveDatas.Count; i++)
                        {
                            var saveData = saveDatas[i];
                            saveFile.AddSaveData(saveData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ERROR("Save", e);
                error = true;
            }

            if (error)
            {
                try
                {
                    if (File.Exists(saveFile.FilePath))
                    {
                        File.Delete(saveFile.FilePath);
                    }
                }
                catch (Exception e)
                {
                    Log.ERROR("Save", e);
                }
            }
        }

        void ISaveDataProvider.Save(ISaveFile saveFile)
        {
            bool error = false;
            try
            {
                using (var fs = new FileStream(saveFile.FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        foreach (var item in saveFile.SaveDatas)
                        {
                            if (item is IBinarySaveData data)
                            {
                                bw.Write(data.GetType().FullName);
                                data.WriteFields(bw);
                            }
                            else
                            {
                                throw new Exception($"{item} is not impl IBinarySaveData");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ERROR("Save", e);
                error = true;
            }

            if (error)
            {
                try
                {
                    if (File.Exists(saveFile.FilePath))
                    {
                        File.Delete(saveFile.FilePath);
                    }
                }
                catch (Exception e)
                {
                    Log.ERROR("Save", e);
                }
            }
        }
    }
}
