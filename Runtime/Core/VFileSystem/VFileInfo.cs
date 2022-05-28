namespace Saro.IO
{
    /// <summary>
    /// 虚拟文件信息
    /// </summary>
    public struct VFileInfo
    {
        private readonly string m_Name;
        private readonly long m_Offset;
        private readonly int m_Length;

        public VFileInfo(string name, long offset, int length)
        {
            if (string.IsNullOrEmpty(name)) throw new System.Exception("name is invalid.");
            if (offset < 0L) throw new System.Exception("offset is invalid.");
            if (length < 0) throw new System.Exception("length is invalid.");

            m_Name = name;
            m_Offset = offset;
            m_Length = length;
        }

        /// <summary>
        /// 虚拟文件名
        /// </summary>
        public string Name => m_Name;

        /// <summary>
        /// 数据偏移
        /// </summary>
        public long Offset => m_Offset;

        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length => m_Length;

        public bool IsValid() => !string.IsNullOrEmpty(m_Name) && m_Offset >= 0L && m_Length > 0;

        public override string ToString() => $"{m_Name}|{m_Offset}|{m_Length}";
    }
}
