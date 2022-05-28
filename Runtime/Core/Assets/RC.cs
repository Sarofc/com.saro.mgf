namespace Saro.Core
{
    public class RC : IRefCount
    {
        public int RefCount => m_RefCount;

        private int m_RefCount;

        public bool IsUnused()
        {
            return m_RefCount <= 0;
        }

        public void IncreaseRefCount()
        {
            m_RefCount++;
        }

        public void DecreaseRefCount()
        {
            m_RefCount--;
        }

        public void SetRefCountForce(int count)
        {
            m_RefCount = count;
        }
    }
}
