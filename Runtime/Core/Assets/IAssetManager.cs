using Cysharp.Threading.Tasks;
using System;
using System.Text;
using UnityEngine;

namespace Saro.Core
{
    public interface IAssetManager : IService
    {
        public static IAssetManager Current => Main.Resolve<IAssetManager>();

        /// <summary>
        /// 加载远端资源状态委托，false:开始加载 true:完成加载
        /// </summary>
        Action<string, bool> OnLoadRemoteAsset { get; set; }

        /// <summary>
        /// 加载远端资源失败委托，统一托管
        /// </summary>
        Action<string> OnLoadRemoteAssetError { get; set; }

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
        [System.Obsolete("use LoadAssetAsync+WaitForCompletion instead", true)]
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
        /// 卸载所有无用资源
        /// </summary>
        /// <param name="force">立即卸载，如果没有实现延迟卸载，可以忽略</param>
        void UnloadUnusedAssets(bool immediate);

        /// <summary>
        /// 删除DLC目录
        /// <code>需要确保，当前未加载dlc里的内容。否则此操作完毕就要退出应用</code>
        /// </summary>
        void DeleteDLC();

        #region RawFile
         
        // TODO 看情况再加吧

        ///// <summary>
        ///// 加载RawFile文本
        ///// </summary>
        ///// <param name="assetName">  是相对路径 eg. xxx.raw
        ///// <returns></returns>
        //string GetRawFileText(string assetName, Encoding encoding = null);

        ///// <summary>
        ///// 异步加载RawFile文本
        ///// </summary>
        ///// <param name="assetName">  是相对路径 eg. Assets/ResRaw/xxx.raw
        ///// <returns></returns>
        //UniTask<string> GetRawFileTextAsync(string assetName, Encoding encoding = null);

        /// <summary>
        /// 加载RawFile字节数组
        /// </summary>
        /// <param name="assetName">  是相对路径 eg. xxx.raw
        /// <returns></returns>
        byte[] GetRawFileBytes(string assetName);

        /// <summary>
        /// 异步加载RawFile字节数组
        /// </summary>
        /// <param name="assetName">  是相对路径 eg. Assets/ResRaw/xxx.raw
        /// <returns></returns>
        UniTask<byte[]> GetRawFileBytesAsync(string assetName);

        /// <summary>
        /// 获取RawFile文件路径
        /// </summary>
        /// <param name="assetName">  是相对路径 eg. Assets/ResRaw/xxx.raw
        /// <returns></returns>
        string GetRawFilePath(string assetName);

        /// <summary>
        /// 获取RawFile文件路径，如没有则会去remote下载。若返回空串，则表示下载失败
        /// </summary>
        /// <param name="assetName">  是相对路径 eg. Assets/ResRaw/xxx.raw
        /// <returns></returns>
        UniTask<string> GetRawFilePathAsync(string assetName);

        #endregion
    }
}