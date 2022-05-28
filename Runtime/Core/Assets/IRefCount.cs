namespace Saro.Core
{
    public interface IRefCount
    {
        /// <summary>
        /// 引用计数
        /// </summary>
        int RefCount { get; }

        /// <summary>
        /// 引用加一
        /// </summary>
        void IncreaseRefCount();

        /// <summary>
        /// 引用减一
        /// </summary>
        void DecreaseRefCount();

        /// <summary>
        /// 强制设置引用数量，可用于 清空引用计数
        /// </summary>
        void SetRefCountForce(int count);

        /// <summary>
        /// 判断有没有引用
        /// </summary>
        /// <returns></returns>
        bool IsUnused();
    }
}
