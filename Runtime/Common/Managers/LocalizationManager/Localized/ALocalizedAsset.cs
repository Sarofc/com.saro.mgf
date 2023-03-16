using System;
using Cysharp.Threading.Tasks;
using Saro.Core;

namespace Saro.Localization
{
    public abstract class ALocalizedAsset<T>
    {
        public virtual string LocalizedKey { get; protected set; }

        /// <summary>
        /// 获取本地化键值对的Value
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        public abstract string GetLocalizedValue();

        /// <summary>
        /// 获取本地化的资源对象
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public abstract UniTask<T> GetLocalizedAssestAsync(IAssetLoader loader);

        // TODO 看看要不要实现 GetLocalizedAssest 同步版本

        public ALocalizedAsset(string localizedKey)
        {
            LocalizedKey = localizedKey;
        }
    }
}
