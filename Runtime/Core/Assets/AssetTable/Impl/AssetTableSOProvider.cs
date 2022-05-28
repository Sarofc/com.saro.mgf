using Cysharp.Threading.Tasks;

namespace Saro.Core
{
    public class AssetTableSOProvider : IAssetTableProvider
    {
        IAssetTable IAssetTableProvider.GetAssetTable(IAssetManager assetManager)
        {
            return assetManager.LoadAsset(AssetTableSO.k_Path, typeof(AssetTableSO)).GetAsset<AssetTableSO>();
        }

        async UniTask<IAssetTable> IAssetTableProvider.GetAssetTableAsync(IAssetManager assetManager)
        {
            var handle = assetManager.LoadAssetAsync(AssetTableSO.k_Path, typeof(AssetTableSO));
            await handle;
            return handle.GetAsset<AssetTableSO>();
        }
    }
}