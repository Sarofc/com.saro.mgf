using Cysharp.Threading.Tasks;
using System;

namespace Saro.Core
{
    public interface IAssetManager : IService
    {
        /// <summary>
        /// 获取应用版本号
        /// </summary>
        /// <returns></returns>
        string GetAppVersion();

        /// <summary>
        /// 获取资源版本号
        /// </summary>
        /// <returns></returns>
        string GetResVersion();

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">加载路径，unity Assets 起始的路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        IAssetHandle LoadAsset(string assetPath, Type type);

        /// <summary>
        /// 异步加载资源 
        /// </summary>
        /// <param name="assetPath">加载路径，unity Assets 起始的路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        IAssetHandle LoadAssetAsync(string assetPath, Type type);

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetPath">加载路径，unity Assets 起始的路径</param>
        /// <param name="additive">是否叠加加载</param>
        /// <returns></returns>
        IAssetHandle LoadSceneAsync(string assetPath, bool additive = false);

        /// <summary>
        /// 是否已经加载，或正在加载某资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        //bool HasAssetLoadedOrLoading(string path, out IAssetHandle handle);

        /// <summary>
        /// 异步检查Custom文件夹资源
        /// </summary>
        /// <param name="assetName">  是相对路径 eg. Custom/xxx.assest
        /// <returns></returns>
        UniTask<string> CheckRawBundlesAsync(string assetName);

        /// <summary>
        /// 加载Custom文件夹资源
        /// </summary>
        /// <param name="assetName">  是相对路径 eg. Custom/xxx.assest
        /// <returns></returns>
        byte[] LoadRawAsset(string assetName);

        /// <summary>
        /// 异步加载Custom文件夹资源
        /// </summary>
        /// <param name="assetName">  是相对路径 eg. Custom/xxx.assest
        /// <returns></returns>
        UniTask<byte[]> LoadRawAssetAsync(string assetName);

        /// <summary>
        /// 卸载所有无用资源
        /// </summary>
        /// <param name="force">立即卸载，如果没有实现延迟卸载，可以忽略</param>
        void UnloadUnusedAssets(bool immediate);

        #region 基于ID的接口，可选实现

        IAssetTable AssetTable { get; }
        
        void LoadAssetTable(IAssetTableProvider provider) => throw new NotImplementedException("基于AssetID的加载接口未实现");

        UniTask LoadAssetTableAsync(IAssetTableProvider provider) => throw new NotImplementedException("基于AssetID的加载接口未实现");

        IAssetHandle LoadAsset(int assetID, Type type) => throw new NotImplementedException("基于AssetID的加载接口未实现");

        IAssetHandle LoadAssetAsync(int assetID, Type type) => throw new NotImplementedException("基于AssetID的加载接口未实现");

        #endregion
    }
}