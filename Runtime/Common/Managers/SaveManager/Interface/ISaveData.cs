using System.IO;

namespace Saro.SaveSystem
{
    public interface ISaveData
    {
    }

    public interface IBinarySaveData : ISaveData
    {
        void WriteFields(BinaryWriter writer);

        void LoadFields(BinaryReader reader);
    }
}
