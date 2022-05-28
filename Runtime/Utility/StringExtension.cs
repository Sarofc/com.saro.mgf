using System;

namespace Saro.Utility
{
    public static class StringExtension
    {
        /// <summary>
        /// String.Replace fast版本
        /// <code>直接修改字符串内存，不再遵循string不可变原则，需要特别注意。</code>
        /// </summary>
        public unsafe static string ReplaceFast(this string self, char oldChar, char newChar)
        {
            fixed (char* pValue = self)
            {
                ReplaceFast(pValue, oldChar, newChar);
                return self;
            }
        }

        private unsafe static void ReplaceFast(char* pValue, char oldChar, char newChar)
        {
            var pCur = pValue;
            while (*pCur != '\0')
            {
                var chr = *pCur;
                if (chr == oldChar)
                {
                    *pCur = newChar;
                }
                pCur++;
            }
        }

        /// <summary>
        /// String.ToLower fast版本
        /// <code>直接修改字符串内存，不再遵循string不可变原则，需要特别注意。</code>
        /// </summary>
        public unsafe static string ToLowerFast(this string self)
        {
            fixed (char* pValue = self)
            {
                ToLowerFast(pValue);
                return self;
            }
        }

        private unsafe static void ToLowerFast(char* pValue)
        {
            var pCur = pValue;
            while (*pCur != '\0')
            {
                var chr = *pCur;
                if ('A' <= chr && chr <= 'Z')
                {
                    *pCur = (char)(chr | 0x20);
                }
                pCur++;
            }
        }

        public static SpanSplitEnumerator<char> SplitFast(this string value, char splitChar)
        {
            var chrSpan = value.AsSpan();
            return chrSpan.Split(splitChar);
        }
    }
}
