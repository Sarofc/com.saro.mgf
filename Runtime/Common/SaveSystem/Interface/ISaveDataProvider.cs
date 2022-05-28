namespace Saro.SaveSystem
{
    public interface ISaveDataProvider
    {
        void Save(ISaveFile saveFile);

        void Load(ISaveFile saveFile);
    }
}
