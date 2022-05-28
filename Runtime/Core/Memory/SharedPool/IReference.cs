
/// <summary>
/// <see cref="SharedPool"/>  接口
/// </summary>
public interface IReference
{
    /// <summary>
    /// 清理引用
    /// <code><see cref="SharedPool.Return(IReference)"/></code>
    /// </summary>
    void IReferenceClear();
}