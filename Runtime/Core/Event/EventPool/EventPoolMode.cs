namespace Saro.Events
{
    [System.Flags]
    public enum EEventPoolMode
    {
        Default = 0,
        /// <summary>
        /// 允许事件处理函数 为空
        /// </summary>
        AllowNoHandler = 1,
        /// <summary>
        /// 允许一个事件ID绑定多个 事件处理函数
        /// </summary>
        AllowMultiHandler = 2,
        /// <summary>
        /// WARNING 允许一个事件ID 重复绑定 同一个事件梳理函数
        /// </summary>
        AllowDuplicateHandler = 4
    }
}