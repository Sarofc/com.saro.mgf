﻿namespace Saro.Core
{
    [System.Obsolete("use 'IAssetLoader' instead")]
    public static class AssetLoaderFactory
    {
        public static T Create<T>(int capacity) where T : class, IAssetLoader, new()
        {
            return IAssetLoader.Create<T>(capacity);
        }

        public static T Create<T>(int capacity, bool poolable) where T : class, IAssetLoader, IReference, new()
        {
            return IAssetLoader.Create<T>(capacity, poolable);
        }

        public static void Release(IAssetLoader assetLoader)
        {
            IAssetLoader.Release(assetLoader);
        }
    }

    partial interface IAssetLoader
    {
        public static T Create<T>(int capacity) where T : class, IAssetLoader, new()
        {
            var loader = new T();
            loader.Poolable = false;
            loader.Init(capacity);
            return loader;
        }

        public static T Create<T>(int capacity, bool poolable) where T : class, IAssetLoader, IReference, new()
        {
            var loader = poolable ? SharedPool.Rent<T>() : new T();
            loader.Poolable = poolable;
            loader.Init(capacity);
            return loader;
        }

        public static void Release(IAssetLoader assetLoader)
        {
            if (assetLoader is IReference reference)
            {
                if (assetLoader.Poolable)
                {
                    SharedPool.Return(reference);
                }
            }
        }
    }
}