using Cysharp.Threading.Tasks;

namespace Saro.Core
{
    /*
     * TODO
     *
     * 1. 给定指定目录，资源名称需要命名位 id_xxx，自动截取 id，生成map<id,path>
     * 2. 需要在合适的时机自动刷新，不能影响正常工作
     * 3. 提供手动刷新按钮
     * 4. 提供editor支持，可以点击按钮，ping指定资源
     * 
     */
    public interface IAssetTableProvider
    {
        IAssetTable GetAssetTable(IAssetManager assetManager);
        UniTask<IAssetTable> GetAssetTableAsync(IAssetManager assetManager);
    }
}