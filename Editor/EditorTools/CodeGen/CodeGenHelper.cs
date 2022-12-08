using System.Text;

namespace Saro.CodeGen
{

    public class CodeGenHelper
    {
        public static string ConvertToValidIdentifier(string name)
        {
            var sb = new StringBuilder(name.Length + 1);

            if (!char.IsLetter(name[0]))
                sb.Append('_');

            var makeUpper = false;
            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(makeUpper
                        ? char.ToUpperInvariant(ch)
                        : ch);
                    makeUpper = false;
                }
                else if (char.IsWhiteSpace(ch))
                {
                    makeUpper = true;
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }
    }
}