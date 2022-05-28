using System;
using System.IO;
using UnityEngine;

// from Game Framework

namespace Saro.IO
{
    /// <summary>
    /// 安卓文件系统流。Only for Android StreammingAssets
    /// </summary>
    public sealed class AndroidVFileStream : VFileStream
    {
        private static readonly string k_SplitFlag = "!/assets/";
        private static readonly int k_SplitFlagLength = k_SplitFlag.Length;
        private static readonly AndroidJavaObject k_AssetManager = null;
        private static readonly IntPtr k_InternalReadMethodId = IntPtr.Zero;
        private static readonly jvalue[] k_InternalReadArgs = null;

        private readonly AndroidJavaObject m_FileStream;
        private readonly IntPtr m_FileStreamRawObject;

        static AndroidVFileStream()
        {
            AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            if (unityPlayer == null)
            {
                throw new IOException("Unity player is invalid.");
            }

            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (currentActivity == null)
            {
                throw new IOException("Current activity is invalid.");
            }

            AndroidJavaObject assetManager = currentActivity.Call<AndroidJavaObject>("getAssets");
            if (assetManager == null)
            {
                throw new IOException("Asset manager is invalid.");
            }

            k_AssetManager = assetManager;

            IntPtr inputStreamClassPtr = AndroidJNI.FindClass("java/io/InputStream");
            k_InternalReadMethodId = AndroidJNIHelper.GetMethodID(inputStreamClassPtr, "read", "([BII)I");
            k_InternalReadArgs = new jvalue[3];

            AndroidJNI.DeleteLocalRef(inputStreamClassPtr);
            currentActivity.Dispose();
            unityPlayer.Dispose();
        }

        /// <summary>
        /// 初始化安卓文件系统流的新实例。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        /// <param name="access">要加载的文件系统的访问方式。</param>
        /// <param name="createNew">是否创建新的文件系统流。</param>
        public AndroidVFileStream(string fullPath, FileMode mode, FileAccess access)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new IOException("Full path is invalid.");
            }

            if (mode != FileMode.Open)
            {
                throw new IOException(string.Format("'{0}' is not supported in AndroidFileStream.", mode));
            }

            if (access != FileAccess.Read)
            {
                throw new IOException(string.Format("'{0}' is not supported in AndroidFileStream.", access));
            }

            int position = fullPath.LastIndexOf(k_SplitFlag, StringComparison.Ordinal);
            if (position < 0)
            {
                throw new IOException("Can not find split flag in full path.");
            }

            string fileName = fullPath.Substring(position + k_SplitFlagLength);
            m_FileStream = InternalOpen(fileName);
            if (m_FileStream == null)
            {
                throw new IOException(string.Format("Open file '{0}' from Android asset manager failure.", fullPath));
            }

            m_FileStreamRawObject = m_FileStream.GetRawObject();
        }

        /// <summary>
        /// 获取或设置文件系统流位置。
        /// </summary>
        protected internal override long Position
        {
            get => throw new IOException("Get position is not supported in AndroidFileStream.");
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <summary>
        /// 获取文件系统流长度。
        /// </summary>
        protected internal override long Length => InternalAvailable();

        /// <summary>
        /// 设置文件系统流长度。
        /// </summary>
        /// <param name="length">要设置的文件系统流的长度。</param>
        protected internal override void SetLength(long length) => throw new IOException("SetLength is not supported in AndroidFileStream.");

        /// <summary>
        /// 定位文件系统流位置。
        /// </summary>
        /// <param name="offset">要定位的文件系统流位置的偏移。</param>
        /// <param name="origin">要定位的文件系统流位置的方式。</param>
        protected internal override void Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
            {
                Seek(Length + offset, SeekOrigin.Begin);
                return;
            }

            if (origin == SeekOrigin.Begin)
            {
                InternalReset();
            }

            while (offset > 0)
            {
                long skip = InternalSkip(offset);
                if (skip < 0)
                {
                    return;
                }

                offset -= skip;
            }
        }

        /// <summary>
        /// 从文件系统流中读取一个字节。
        /// </summary>
        /// <returns>读取的字节，若已经到达文件结尾，则返回 -1。</returns>
        protected internal override int ReadByte() => InternalRead();

        /// <summary>
        /// 从文件系统流中读取二进制流。
        /// </summary>
        /// <param name="buffer">存储读取文件内容的二进制流。</param>
        /// <param name="startIndex">存储读取文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储读取文件内容的二进制流的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        protected internal override int Read(byte[] buffer, int startIndex, int length)
        {
            int bytesRead = InternalRead(length, out var result);
            Array.Copy(result, 0, buffer, startIndex, bytesRead);
            return bytesRead;
        }

        /// <summary>
        /// 向文件系统流中写入一个字节。
        /// </summary>
        /// <param name="value">要写入的字节。</param>
        protected internal override void WriteByte(byte value) => throw new IOException("WriteByte is not supported in AndroidFileStream.");

        /// <summary>
        /// 向文件系统流中写入二进制流。
        /// </summary>
        /// <param name="buffer">存储写入文件内容的二进制流。</param>
        /// <param name="startIndex">存储写入文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储写入文件内容的二进制流的长度。</param>
        protected internal override void Write(byte[] buffer, int startIndex, int length) => throw new IOException("Write is not supported in AndroidFileStream.");

        /// <summary>
        /// 将文件系统流立刻更新到存储介质中。
        /// </summary>
        protected internal override void Flush() => throw new IOException("Flush is not supported in AndroidFileStream.");

        /// <summary>
        /// 关闭文件系统流。
        /// </summary>
        protected internal override void Close()
        {
            InternalClose();
            m_FileStream.Dispose();
        }

        private AndroidJavaObject InternalOpen(string fileName)
            => k_AssetManager.Call<AndroidJavaObject>("open", fileName);

        private int InternalAvailable() => m_FileStream.Call<int>("available");

        private void InternalClose() => m_FileStream.Call("close");

        private int InternalRead() => m_FileStream.Call<int>("read");

        private int InternalRead(int length, out byte[] result)
        {
#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            IntPtr resultPtr = AndroidJNI.NewByteArray(length);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            int offset = 0;
            int bytesLeft = length;
            while (bytesLeft > 0)
            {
                k_InternalReadArgs[0] = new jvalue() { l = resultPtr };
                k_InternalReadArgs[1] = new jvalue() { i = offset };
                k_InternalReadArgs[2] = new jvalue() { i = bytesLeft };
                int bytesRead = AndroidJNI.CallIntMethod(m_FileStreamRawObject, k_InternalReadMethodId, k_InternalReadArgs);
                if (bytesRead <= 0)
                {
                    break;
                }

                offset += bytesRead;
                bytesLeft -= bytesRead;
            }

#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            result = AndroidJNI.FromByteArray(resultPtr);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            AndroidJNI.DeleteLocalRef(resultPtr);
            return offset;
        }

        private void InternalReset() => m_FileStream.Call("reset");

        private long InternalSkip(long offset) => m_FileStream.Call<long>("skip", offset);

        public override void Dispose() => Close();
    }
}
