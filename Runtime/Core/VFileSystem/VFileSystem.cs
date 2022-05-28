using Saro.Collections;
using Saro.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Saro.IO
{
    /*
     * TODO
     *
     * 1. API 优化，太冗余了
     * 2. 碎片整理功能
     * 3. async api，将io操作串行起来，参考bcl，使用Memory<T>
     */

    /*
     * 虚拟文件系统
     * 提供 CRUD
     * 可用于优化细碎文件
     */
    public sealed partial class VFileSystem : IDisposable
    {
        private const int k_ClusterSize = 1024 * 4;
        private const int k_CachedBytesLength = 0x1000;

        private static readonly byte[] s_CachedBytes = new byte[k_CachedBytesLength];

        private static readonly int s_HeaderDataSize = Marshal.SizeOf(typeof(HeaderData));
        private static readonly int s_BlockDataSize = Marshal.SizeOf(typeof(BlockData));
        private static readonly int s_StringDataSize = Marshal.SizeOf(typeof(StringData));

        private readonly string m_FullPath;
        private readonly FileAccess m_Access;
        private readonly VFileStream m_Stream;

        private HeaderData m_HeaderData;

        /// <summary>
        /// 文件索引表
        /// <para>k: <see cref="VFileInfo.Name"/></para>
        /// <para>v: <see cref="VFileSystem.m_BlockDatas"/></para>
        /// </summary>
        private readonly Dictionary<string, int> m_FileIndexesMap;
        /// <summary>
        /// 数据块容器
        /// </summary>
        private readonly List<BlockData> m_BlockDatas;
        /// <summary>
        /// 可用文件索引
        /// <para>k: <see cref="BlockData.Length"/></para>
        /// <para>v: <see cref="VFileSystem.m_BlockDatas"/></para>
        /// </summary>
        private readonly TMultiMap<int, int> m_FreeBlockIndexesMap;
        /// <summary>
        /// 字符串数据索引表
        /// <para>k: <see cref="BlockData.StringIndex"/></para>
        /// <para>v: <see cref="StringData"/></para>
        /// </summary>
        private readonly SortedDictionary<int, StringData> m_StringDataMap;
        /// <summary>
        /// 可用字符串索引
        /// </summary>
        private readonly Queue<KeyValuePair<int, StringData>> m_FreeStringDatas;

        /// <summary>
        /// 数据块偏移
        /// </summary>
        private int m_BlockDataOffsest;
        /// <summary>
        /// 字符串数据偏移
        /// </summary>
        private int m_StringDataOffset;
        /// <summary>
        /// 文件数据偏移
        /// </summary>
        private int m_FileDataOffset;

        /*
         * ++++++++++++++++++++ -> 0
         * +    HeaderData    +
         * ++++++++++++++++++++ -> m_BlockDataOffsest
         * +    BlockData     +
         * +------------------+
         * +    BlockData     +
         * +------------------+
         * +    BlockData     +
         * +------------------+
         * +    ...           +
         * ++++++++++++++++++++ -> m_StringDataOffset
         * +    StringData    +
         * +------------------+
         * +    StringData    +
         * +------------------+
         * +    StringData    +
         * +------------------+
         * +    ...           +
         * ++++++++++++++++++++ -> m_FileDataOffset
         * +      bytes       +
         * +------------------+
         * +                  +
         * +      bytes       +
         * +                  +
         * +------------------+
         * +      bytes       +
         * +------------------+
         * +      bytes       +
         * +                  +
         * +------------------+
         * +      ...         +
         * ++++++++++++++++++++
         */

        private VFileSystem(string fullPath, FileAccess access, VFileStream stream)
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new Exception("fullpath is invalid.");

            if (stream == null)
                throw new Exception("stream is invalid.");

            m_FullPath = fullPath;
            m_Access = access;
            m_Stream = stream;
            m_FileIndexesMap = new Dictionary<string, int>(StringComparer.Ordinal);
            m_BlockDatas = new List<BlockData>();
            m_FreeBlockIndexesMap = new TMultiMap<int, int>();
            m_StringDataMap = new SortedDictionary<int, StringData>();
            m_FreeStringDatas = new Queue<KeyValuePair<int, StringData>>();

            m_HeaderData = default;
            m_BlockDataOffsest = 0;
            m_StringDataOffset = 0;
            m_FileDataOffset = 0;
        }

        /// <summary>
        /// 虚拟文件路径
        /// </summary>
        public string FullPath => m_FullPath;

        /// <summary>
        /// 虚拟文件权限
        /// </summary>
        public FileAccess Access => m_Access;

        /// <summary>
        /// 包含文件数量
        /// </summary>
        public int FileCount => m_FileIndexesMap.Count;

        /// <summary>
        /// 最大支持文件数量
        /// </summary>
        public int MaxFileCount => m_HeaderData.MaxFileCount;

        public static VFileSystem Open(string fullPath, FileMode mode, FileAccess access, int maxFileCount = 1024, int maxBlockCount = 1024)
        {
            if (mode == FileMode.Create ||
                mode == FileMode.Append ||
                mode == FileMode.Truncate)
            {
                throw new NotSupportedException($"VFileSystem {mode} is not supported");
            }

            bool fileExit = FileUtility.Exists(fullPath);

            VFileStream stream = null;

            try
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (FileUtility.IsAndroidStreammingAssetPath(fullPath))
                {
                    stream = new AndroidVFileStream(fullPath, mode, access);
                }
                else
                {
                    stream = new CommonVFileStream(fullPath, mode, access);
                }
#else
                stream = new CommonVFileStream(fullPath, mode, access);
#endif
            }
            catch (Exception e)
            {
                stream?.Dispose();
                Log.ERROR(e);
            }

            try
            {
                return fileExit ? OpenInternal(fullPath, access, stream) : CreateInternal(fullPath, access, stream, maxFileCount, maxBlockCount);
            }
            catch (Exception e)
            {
                stream?.Dispose();
                Log.ERROR(e);
            }

            return null;
        }

        /// <summary>
        /// 创建虚拟文件
        /// </summary>
        /// <param name="fullPath">路径</param>
        /// <param name="access">权限</param>
        /// <param name="stream">流</param>
        /// <param name="maxFileCount">最大支持文件数</param>
        /// <param name="maxBlockCount">最大支持文件数 <code>see <see cref="FileCount"/> x (2~32) 较为合适</code></param>
        /// <returns></returns>
        private static VFileSystem CreateInternal(string fullPath, FileAccess access, VFileStream stream, int maxFileCount, int maxBlockCount)
        {
            if (maxFileCount <= 0)
                throw new Exception("max file count is invalid.");

            if (maxBlockCount <= 0)
                throw new Exception("max block count is invalid.");

            if (maxFileCount > maxBlockCount)
                throw new Exception("must maxFileCount <= maxBlockCount");

            VFileSystem fileSystem = new VFileSystem(fullPath, access, stream);
            fileSystem.m_HeaderData = new HeaderData(maxFileCount, maxBlockCount);
            CalcOffsets(fileSystem);

            Utility.MemoryUtility.StructureToBytes(fileSystem.m_HeaderData, s_CachedBytes, 0, s_HeaderDataSize);

            stream.Write(s_CachedBytes, 0, s_HeaderDataSize);
            stream.SetLength(fileSystem.m_FileDataOffset);
            return fileSystem;
        }

        /// <summary>
        /// 加载虚拟文件
        /// </summary>
        /// <param name="fullPath">路径</param>
        /// <param name="access">权限</param>
        /// <param name="stream">流</param>
        /// <returns></returns>
        private static VFileSystem OpenInternal(string fullPath, FileAccess access, VFileStream stream)
        {
            VFileSystem fileSystem = new VFileSystem(fullPath, access, stream);

            stream.Read(s_CachedBytes, 0, s_HeaderDataSize);

            fileSystem.m_HeaderData = Utility.MemoryUtility.BytesToStructure<HeaderData>(s_CachedBytes, 0, s_HeaderDataSize);

            if (!fileSystem.m_HeaderData.IsValid())
                throw new Exception($"invalid file: {fullPath} \ninfo: {fileSystem.m_HeaderData}");

            CalcOffsets(fileSystem);

            if (fileSystem.m_BlockDatas.Capacity < fileSystem.m_HeaderData.BlockCount)
            {
                fileSystem.m_BlockDatas.Capacity = fileSystem.m_HeaderData.BlockCount;
            }

            for (int i = 0; i < fileSystem.m_HeaderData.BlockCount; i++)
            {
                stream.Read(s_CachedBytes, 0, s_BlockDataSize);

                BlockData blockData = Utility.MemoryUtility.BytesToStructure<BlockData>(s_CachedBytes, 0, s_BlockDataSize);

                fileSystem.m_BlockDatas.Add(blockData);
            }

            for (int i = 0; i < fileSystem.m_BlockDatas.Count; i++)
            {
                BlockData blockData = fileSystem.m_BlockDatas[i];
                if (blockData.Using)
                {
                    StringData stringData = fileSystem.ReadStringData(blockData.StringIndex);
                    fileSystem.m_StringDataMap.Add(blockData.StringIndex, stringData);
                    fileSystem.m_FileIndexesMap.Add(stringData.GetString(fileSystem.m_HeaderData.GetEncryptBytes()), i);
                }
                else
                {
                    fileSystem.m_FreeBlockIndexesMap.Add(blockData.Length, i);
                }
            }

            return fileSystem;
        }

        public void Dispose()
        {
            m_Stream.Close();

            m_FileIndexesMap.Clear();
            m_BlockDatas.Clear();
            m_FreeBlockIndexesMap.Clear();
            m_StringDataMap.Clear();
            m_FreeStringDatas.Clear();

            m_BlockDataOffsest = 0;
            m_StringDataOffset = 0;
            m_FileDataOffset = 0;

#if false
#if UNITY_EDITOR
            UnityEngine.Debug.LogError("VFileSystem::Dispose");
#else
            Console.WriteLine("VFileSystem::Dispose");
#endif
#endif
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="name">�ļ���</param>
        /// <returns></returns>
        public VFileInfo GetFileInfo(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (!m_FileIndexesMap.TryGetValue(name, out int blockIndex))
            {
                return default;
            }

            BlockData blockData = m_BlockDatas[blockIndex];
            return new VFileInfo(name, GetClusterOffset(blockData.ClusterIndex), blockData.Length);
        }

        /// <summary>
        /// 获取所有文件信息
        /// </summary>
        /// <returns></returns>
        public VFileInfo[] GetAllFileInfos()
        {
            int index = 0;
            VFileInfo[] results = new VFileInfo[m_FileIndexesMap.Count];
            foreach (var kv in m_FileIndexesMap)
            {
                BlockData blockData = m_BlockDatas[kv.Value];
                results[index++] = new VFileInfo(kv.Key, GetClusterOffset(blockData.ClusterIndex), blockData.Length);
            }
            return results;
        }

        /// <summary>
        /// 获取所有文件信息 <code>NonAlloc</code>
        /// </summary>
        /// <param name="results"></param>
        public void GetAllFileInfos(IList<VFileInfo> results)
        {
            if (results == null)
                throw new Exception("results is invalid.");

            results.Clear();
            foreach (var kv in m_FileIndexesMap)
            {
                BlockData blockData = m_BlockDatas[kv.Value];
                results.Add(new VFileInfo(kv.Key, GetClusterOffset(blockData.ClusterIndex), blockData.Length));
            }
        }

        /// <summary>
        /// 是否包含某文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns></returns>
        public bool HasFile(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            return m_FileIndexesMap.ContainsKey(name);
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns></returns>
        public byte[] ReadFile(string name)
        {
            if (m_Access != FileAccess.Read
                && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not readable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            VFileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid()) return null;

            int length = fileInfo.Length;
            byte[] buffer = new byte[length];
            if (length > 0)
            {
                m_Stream.Position = fileInfo.Offset;
                m_Stream.Read(buffer, 0, length);
            }

            return buffer;
        }

        /// <summary>
        /// 读取文件 <code>NonAlloc</code>
        /// </summary>
        /// <param name="name">文件名 <see cref="VFileInfo.Name"/></param>
        /// <param name="buffer">数据缓冲</param>
        /// <param name="offset">文件偏移 <see cref="VFileInfo.Offset"/></param>
        /// <param name="count">文件长度 <see cref="VFileInfo.Length"/></param>
        /// <returns>读取了多少字节</returns>
        public int ReadFile(string name, byte[] buffer, int offset = 0, int count = 0)
        {
            if (m_Access != FileAccess.Read
                && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not readable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (buffer == null)
                throw new Exception("buffer is invalid.");

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new Exception("index is invalid.");

            VFileInfo fileInfo = GetFileInfo(name);

            if (!fileInfo.IsValid()) return 0;

            m_Stream.Position = fileInfo.Offset;
            if (count > fileInfo.Length)
            {
                count = fileInfo.Length;
            }

            if (count > 0)
            {
                return m_Stream.Read(buffer, offset, count);
            }

            return 0;
        }

        /// <summary>
        /// 读取文件 <code>NonAlloc</code>
        /// </summary>
        /// <param name="name">文件名 <see cref="VFileInfo.Name"/></param>
        /// <param name="stream">流</param>
        /// <returns>读取了多少字节</returns>
        public int ReadFile(string name, Stream stream)
        {
            if (m_Access != FileAccess.Read
                && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not readable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (stream == null)
                throw new Exception("stream is invalid.");

            if (!stream.CanWrite)
                throw new Exception("stream should canWrite.");

            VFileInfo fileInfo = GetFileInfo(name);

            if (!fileInfo.IsValid()) return 0;

            int length = fileInfo.Length;
            if (length > 0)
            {
                m_Stream.Position = fileInfo.Offset;
                return m_Stream.Read(stream, length);
            }

            return 0;
        }

        /// <summary>
        /// 读取文件片段
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="startIndex">开始索引 <code>FileInfo.Offset + startIndex</code></param>
        /// <param name="count">读取长度</param>
        /// <returns></returns>
        public byte[] ReadFileSegment(string name, int startIndex, int count)
        {
            if (m_Access != FileAccess.Read
                   && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not readable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (startIndex < 0)
                throw new Exception("startIndex is invalid.");

            if (count < 0)
                throw new Exception("count should canWrite.");

            VFileInfo fileInfo = GetFileInfo(name);

            if (!fileInfo.IsValid()) return null;

            if (startIndex > fileInfo.Length)
            {
                startIndex = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - startIndex;
            if (count > leftLength)
            {
                count = leftLength;
            }

            byte[] buffer = new byte[count];
            if (count > 0)
            {
                m_Stream.Position = fileInfo.Offset + startIndex;
                m_Stream.Read(buffer, 0, count);
            }
            return buffer;
        }

        /// <summary>
        /// 读取文件片段
        /// </summary>
        /// /// <param name="name">文件名</param>
        /// <param name="startIndex">开始索引 <code>FileInfo.Offset + startIndex</code></param>
        /// <param name="buffer"></param>
        /// <param name="offset">buffer 中的字节偏移量，将在此处放置读取的字节</param>
        /// <param name="count">读取长度</param>
        /// <returns>读取了多少字节</returns>
        public int ReadFileSegment(string name, int startIndex, byte[] buffer, int offset, int count)
        {
            if (m_Access != FileAccess.Read
                   && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not readable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (buffer == null)
                throw new Exception("buffer is invalid.");

            if (startIndex < 0)
                throw new Exception("startIndex is invalid.");

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new Exception("index is invalid.");

            VFileInfo fileInfo = GetFileInfo(name);

            if (!fileInfo.IsValid()) return 0;

            if (startIndex > fileInfo.Length)
            {
                startIndex = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - startIndex;
            if (count > leftLength)
            {
                count = leftLength;
            }

            if (count > 0)
            {
                m_Stream.Position = fileInfo.Offset + startIndex;
                return m_Stream.Read(buffer, offset, count);
            }

            return 0;
        }

        /// <summary>
        /// 读取文件片段
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="startIndex">开始索引 <code>FileInfo.Offset + startIndex</code></param>
        /// <param name="stream">流</param>
        /// <param name="count">长度</param>
        /// <returns>读取了多少字节</returns>
        public int ReadFileSegment(string name, int startIndex, Stream stream, int count)
        {
            if (m_Access != FileAccess.Read
                   && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not readable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (stream == null)
                throw new Exception("stream is invalid.");

            if (startIndex < 0)
                throw new Exception("offset is invalid.");

            if (count < 0)
                throw new Exception("length is invalid.");

            VFileInfo fileInfo = GetFileInfo(name);

            if (!fileInfo.IsValid()) return 0;

            if (startIndex > fileInfo.Length)
            {
                startIndex = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - startIndex;
            if (count > leftLength)
            {
                count = leftLength;
            }

            if (count > 0)
            {
                m_Stream.Position = fileInfo.Offset + startIndex;
                return m_Stream.Read(stream, count);
            }

            return 0;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="buffer">字节数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="count">长度</param>
        /// <returns>写入成功与否</returns>
        public bool WriteFile(string name, byte[] buffer, int offset, int count)
        {
            if (m_Access != FileAccess.Write
                   && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not writable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (name.Length > byte.MaxValue)
                throw new Exception($"name {name} is too long.");

            if (buffer == null)
                throw new Exception("buffer is invalid.");

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new Exception("index is invalid.");

            bool hasFile = m_FileIndexesMap.TryGetValue(name, out int oldBlockIndex);

            if (!hasFile && m_FileIndexesMap.Count >= m_HeaderData.MaxFileCount)
            {
                return false;
            }

            int blockIndex = AllocBlock(count);
            if (blockIndex < 0) return false;

            if (count > 0)
            {
                m_Stream.Position = GetClusterOffset(m_BlockDatas[blockIndex].ClusterIndex);
                m_Stream.Write(buffer, offset, count);
            }

            ProcessWriteFile(name, hasFile, oldBlockIndex, blockIndex, count);
            m_Stream.Flush();
            return true;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <param name="stream">流</param>
        /// <returns>写入成功与否</returns>
        public bool WriteFile(string name, Stream stream)
        {
            if (m_Access != FileAccess.Write
                && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not writable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (stream == null)
                throw new Exception("stream is invalid.");

            if (!stream.CanRead)
                throw new Exception("stream should CanRead.");

            if (name.Length > byte.MaxValue)
                throw new Exception($"Name {name} is too long.");

            bool hasFile = m_FileIndexesMap.TryGetValue(name, out int oldBlockIndex);

            if (!hasFile && m_FileIndexesMap.Count >= m_HeaderData.MaxFileCount)
            {
                return false;
            }

            int length = (int)(stream.Length - stream.Position);
            int blockIndex = AllocBlock(length);
            if (blockIndex < 0)
            {
                return false;
            }

            if (length > 0)
            {
                m_Stream.Position = GetClusterOffset(m_BlockDatas[blockIndex].ClusterIndex);
                m_Stream.Write(stream, length);
            }

            ProcessWriteFile(name, hasFile, oldBlockIndex, blockIndex, length);
            m_Stream.Flush();
            return true;
        }

        public bool WriteFile(string name, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException($"filePath is null or empty.");

            if (!File.Exists(filePath))
            {
                return false;
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return WriteFile(name, fileStream);
            }
        }

        /// <summary>
        /// 重命名文件
        /// </summary>
        /// <param name="oldName">待命名文件名</param>
        /// <param name="newName">新文件名</param>
        /// <returns>�ɹ����</returns>
        public bool RenameFile(string oldName, string newName)
        {
            if (m_Access != FileAccess.Write
                   && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not writable.");

            if (string.IsNullOrEmpty(oldName))
                throw new Exception("oldName is invalid.");

            if (string.IsNullOrEmpty(newName))
                throw new Exception("newName is invalid.");

            if (newName.Length > byte.MaxValue)
                throw new Exception($"newName {newName} is too long.");

            if (oldName == newName) return true;

            if (m_FileIndexesMap.ContainsKey(newName)) return false;

            if (!m_FileIndexesMap.TryGetValue(oldName, out var blockIndex)) return false;

            int stringIndex = m_BlockDatas[blockIndex].StringIndex;
            StringData stringData = m_StringDataMap[stringIndex].SetString(newName, m_HeaderData.GetEncryptBytes());
            m_StringDataMap[stringIndex] = stringData;
            WriteStringData(stringIndex, stringData);
            m_FileIndexesMap.Add(newName, blockIndex);
            m_FileIndexesMap.Remove(oldName);
            m_Stream.Flush();
            return true;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>删除成功与否</returns>
        public bool DeleteFile(string name)
        {
            if (m_Access != FileAccess.Write
                      && m_Access != FileAccess.ReadWrite)
                throw new Exception("file system is not writable.");

            if (string.IsNullOrEmpty(name))
                throw new Exception("name is invalid.");

            if (!m_FileIndexesMap.TryGetValue(name, out int blockIndex)) return false;

            m_FileIndexesMap.Remove(name);

            BlockData blockData = m_BlockDatas[blockIndex];
            int stringIndex = blockData.StringIndex;
            StringData stringData = m_StringDataMap[stringIndex].Clear();
            m_FreeStringDatas.Enqueue(new KeyValuePair<int, StringData>(stringIndex, stringData));
            m_StringDataMap.Remove(stringIndex);
            WriteStringData(stringIndex, stringData);

            blockData = blockData.Free();
            m_BlockDatas[blockIndex] = blockData;
            if (!TryCombineFreeBlocks(blockIndex))
            {
                m_FreeBlockIndexesMap.Add(blockData.Length, blockIndex);
                WriteBlockData(blockIndex);
            }

            m_Stream.Flush();
            return true;
        }

        private void ProcessWriteFile(string name, bool hasFile, int oldBlockIndex, int blockIndex, int length)
        {
            BlockData blockData = m_BlockDatas[blockIndex];
            if (hasFile)
            {
                BlockData oldBlockData = m_BlockDatas[oldBlockIndex];
                blockData = new BlockData(oldBlockData.StringIndex, blockData.ClusterIndex, length);
                m_BlockDatas[blockIndex] = blockData;
                WriteBlockData(blockIndex);

                oldBlockData = oldBlockData.Free();
                m_BlockDatas[oldBlockIndex] = oldBlockData;
                if (!TryCombineFreeBlocks(oldBlockIndex))
                {
                    m_FreeBlockIndexesMap.Add(oldBlockData.Length, oldBlockIndex);
                    WriteBlockData(oldBlockIndex);
                }
            }
            else
            {
                int stringIndex = AllocString(name);
                blockData = new BlockData(stringIndex, blockData.ClusterIndex, length);
                m_BlockDatas[blockIndex] = blockData;
                WriteBlockData(blockIndex);
            }

            m_FileIndexesMap[name] = blockIndex;
        }

        private bool TryCombineFreeBlocks(int freeBlockIndex)
        {
            BlockData freeBlockData = m_BlockDatas[freeBlockIndex];
            if (freeBlockData.Length <= 0) return false;

            int previousFreeBlockIndex = -1;
            int nextFreeBlockIndex = -1;
            int nextBockDataClusterIndex = freeBlockData.ClusterIndex + GetUpBoundClusterCount(freeBlockData.Length);

            foreach (var blockIndexes in m_FreeBlockIndexesMap)
            {
                if (blockIndexes.Key <= 0) continue;

                int blockDataClusterCount = GetUpBoundClusterCount(blockIndexes.Key);

                foreach (var blockIndex in blockIndexes.Value)
                {
                    BlockData blockData = m_BlockDatas[blockIndex];
                    if (blockData.ClusterIndex + blockDataClusterCount == freeBlockData.ClusterIndex)
                    {
                        previousFreeBlockIndex = blockIndex;
                    }
                    else if (blockData.ClusterIndex == nextBockDataClusterIndex)
                    {
                        nextFreeBlockIndex = blockIndex;
                    }
                }
            }

            if (previousFreeBlockIndex < 0 && nextFreeBlockIndex < 0) return false;

            m_FreeBlockIndexesMap.Remove(freeBlockData.Length, freeBlockIndex);
            if (previousFreeBlockIndex >= 0)
            {
                BlockData previousFreeBlockData = m_BlockDatas[previousFreeBlockIndex];
                m_FreeBlockIndexesMap.Remove(previousFreeBlockData.Length, previousFreeBlockIndex);
                freeBlockData = new BlockData(previousFreeBlockData.ClusterIndex, previousFreeBlockData.Length + freeBlockData.Length);
                m_BlockDatas[previousFreeBlockIndex] = BlockData.s_Empty;
                m_FreeBlockIndexesMap.Add(0, previousFreeBlockIndex);
                WriteBlockData(previousFreeBlockIndex);
            }

            if (nextFreeBlockIndex >= 0)
            {
                BlockData nextFreeBlockData = m_BlockDatas[nextFreeBlockIndex];
                m_FreeBlockIndexesMap.Remove(nextFreeBlockData.Length, nextFreeBlockIndex);
                freeBlockData = new BlockData(freeBlockData.ClusterIndex, freeBlockData.Length + nextFreeBlockData.Length);
                m_BlockDatas[nextFreeBlockIndex] = BlockData.s_Empty;
                m_FreeBlockIndexesMap.Add(0, nextFreeBlockIndex);
                WriteBlockData(nextFreeBlockIndex);
            }

            m_BlockDatas[freeBlockIndex] = freeBlockData;
            m_FreeBlockIndexesMap.Add(freeBlockData.Length, freeBlockIndex);
            WriteBlockData(freeBlockIndex);
            return true;
        }

        private int GetEmptyBlockIndex()
        {
            if (m_FreeBlockIndexesMap.TryGetValue(0, out var lengthRange))
            {
                int blockIndex = lengthRange.Head.Value;
                m_FreeBlockIndexesMap.Remove(0, blockIndex);
                return blockIndex;
            }

            if (m_BlockDatas.Count < m_HeaderData.MaxBlockCount)
            {
                int blockIndex = m_BlockDatas.Count;
                m_BlockDatas.Add(BlockData.s_Empty);
                WriteHeaderData();
                return blockIndex;
            }

            return -1;
        }

        private void WriteHeaderData()
        {
            m_HeaderData = m_HeaderData.SetBlockCount(m_BlockDatas.Count);

            Utility.MemoryUtility.StructureToBytes(m_HeaderData, s_CachedBytes, 0, s_HeaderDataSize);

            m_Stream.Position = 0L;
            m_Stream.Write(s_CachedBytes, 0, s_HeaderDataSize);
        }

        private void WriteBlockData(int blockIndex)
        {
            Utility.MemoryUtility.StructureToBytes(m_BlockDatas[blockIndex], s_CachedBytes, 0, s_BlockDataSize);

            m_Stream.Position = m_BlockDataOffsest + s_BlockDataSize * blockIndex;
            m_Stream.Write(s_CachedBytes, 0, s_BlockDataSize);
        }


        private StringData ReadStringData(int stringIndex)
        {
            m_Stream.Position = m_StringDataOffset + s_StringDataSize * stringIndex;
            m_Stream.Read(s_CachedBytes, 0, s_StringDataSize);

            return Utility.MemoryUtility.BytesToStructure<StringData>(s_CachedBytes, 0, s_StringDataSize);
        }

        private void WriteStringData(int stringIndex, StringData stringData)
        {
            Utility.MemoryUtility.StructureToBytes(stringData, s_CachedBytes, 0, s_StringDataSize);

            m_Stream.Position = m_StringDataOffset + s_StringDataSize * stringIndex;
            m_Stream.Write(s_CachedBytes, 0, s_StringDataSize);
        }


        private int AllocString(string val)
        {
            int stringIndex = -1;
            StringData stringData = default;
            if (m_FreeStringDatas.Count > 0)
            {
                var freeStringData = m_FreeStringDatas.Dequeue();
                stringIndex = freeStringData.Key;
                stringData = freeStringData.Value;
            }
            else
            {
                int index = 0;
                foreach (var item in m_StringDataMap)
                {
                    if (item.Key == index)
                    {
                        index++;
                        continue;
                    }

                    break;
                }

                if (index < m_HeaderData.MaxFileCount)
                {
                    stringIndex = index;
                    byte[] bytes = new byte[byte.MaxValue];

                    // TODO ֻ这里随机看上去啥用
                    //Utility.RandomUtility.NextBytes(bytes);

                    stringData = new StringData(0, bytes);
                }
            }

            if (stringIndex < 0)
                throw new Exception("alloc string internal error.");

            stringData = stringData.SetString(val, m_HeaderData.GetEncryptBytes());

            m_StringDataMap.Add(stringIndex, stringData);
            WriteStringData(stringIndex, stringData);
            return stringIndex;
        }


        private int AllocBlock(int length)
        {
            if (length <= 0) return GetEmptyBlockIndex();

            length = (int)GetUpBoundClusterOffset(length);

            int lengthFound = -1;
            TLinkedListRange<int> lengthRange = default;

            foreach (var indexes in m_FreeBlockIndexesMap)
            {
                if (indexes.Key < length) continue;

                if (lengthFound >= 0 && lengthFound < indexes.Key) continue;

                lengthFound = indexes.Key;
                lengthRange = indexes.Value;

                break;
            }

            if (lengthFound >= 0)
            {
                if (lengthFound > length && m_BlockDatas.Count >= m_HeaderData.MaxBlockCount)
                {
                    return -1;
                }

                int blockIndex = lengthRange.Head.Value;
                m_FreeBlockIndexesMap.Remove(lengthFound, blockIndex);
                if (lengthFound > length)
                {
                    BlockData blockData = m_BlockDatas[blockIndex];
                    m_BlockDatas[blockIndex] = new BlockData(blockData.ClusterIndex, length);
                    WriteBlockData(blockIndex);

                    int deltaLength = lengthFound - length;
                    int anotherBlockIndex = GetEmptyBlockIndex();
                    m_BlockDatas[anotherBlockIndex] = new BlockData(blockData.ClusterIndex + GetUpBoundClusterCount(length), deltaLength);
                    m_FreeBlockIndexesMap.Add(deltaLength, anotherBlockIndex);
                    WriteBlockData(anotherBlockIndex);
                }
                return blockIndex;
            }
            else
            {
                int blockIndex = GetEmptyBlockIndex();
                if (blockIndex < 0) return -1;

                long fileLength = m_Stream.Length;
                try
                {
                    m_Stream.SetLength(fileLength + length);
                }
                catch
                {
                    return -1;
                }

                m_BlockDatas[blockIndex] = new BlockData(GetUpBoundClusterCount(fileLength), length);
                WriteBlockData(blockIndex);
                return blockIndex;
            }
        }

        private static void CalcOffsets(VFileSystem fileSystem)
        {
            fileSystem.m_BlockDataOffsest = s_HeaderDataSize;
            fileSystem.m_StringDataOffset = fileSystem.m_BlockDataOffsest + s_BlockDataSize * fileSystem.m_HeaderData.MaxBlockCount;
            fileSystem.m_FileDataOffset = (int)GetUpBoundClusterOffset(fileSystem.m_StringDataOffset + s_StringDataSize * fileSystem.m_HeaderData.MaxFileCount);
        }

        private static int GetUpBoundClusterCount(long length)
        {
            return (int)((length - 1L + k_ClusterSize) / k_ClusterSize);
        }

        private static long GetUpBoundClusterOffset(long offset)
        {
            return (offset - 1L + k_ClusterSize) / k_ClusterSize * k_ClusterSize;
        }

        private static long GetClusterOffset(int clusterIndex)
        {
            return (long)k_ClusterSize * clusterIndex;
        }
    }
}
