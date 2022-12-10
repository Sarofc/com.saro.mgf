namespace Saro.Saving
{
    public interface ISaveDataProvider
    {
        void Save(ISaveFile saveFile);

        void Load(ISaveFile saveFile);
    }
}
