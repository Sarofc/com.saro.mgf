using System.IO;
using System.Text;

namespace Saro.Utility
{
    public class FileEncodingUtil
    {
        private static byte[] s_UTF8Bom = Encoding.UTF8.GetPreamble();

        public static Encoding GetEncoding(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                return GetEncoding(fs);
            }
        }

        public static Encoding GetEncoding(Stream fs)
        {
            //byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            //byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            //byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM   
            Encoding reVal = Encoding.Default;

            if (fs.Length < 4) return reVal;

            using (BinaryReader r = new BinaryReader(fs, Encoding.Default))
            {
                byte[] ss = r.ReadBytes(4);
                if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                {
                    reVal = Encoding.BigEndianUnicode;
                }
                else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
                {
                    reVal = Encoding.Unicode;
                }
                else
                {
                    if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
                    {
                        reVal = Encoding.UTF8;
                    }
                    else
                    {
                        if (int.TryParse(fs.Length.ToString(), out int i))
                        {
                            ss = r.ReadBytes(i);

                            if (IsUTF8WithoutBOM(ss))
                                reVal = new UTF8Encoding(false);
                        }
                    }
                }
            }
            return reVal;
        }

        public static bool IsUTF8WithoutBOM(byte[] data)
        {
            if (s_UTF8Bom.Length > data.Length) return false;
            for (int i = 0; i < s_UTF8Bom.Length; i++)
            {
                if (s_UTF8Bom[i] != data[i]) return false;
            }
            return true;
        }
    }
}