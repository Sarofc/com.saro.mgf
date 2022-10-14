using System;
using System.Text;
using UnityEngine;

namespace Saro.XConsole
{
    internal static class StringBuilderExtension
    {
        public static StringBuilder AppendColorText(this StringBuilder sb, string text, Color color)
        {
            sb.Append("<color=#")
                .Append(ColorUtility.ToHtmlStringRGB(color))
                .Append(">")
                .Append(text)
                .Append("</color>");
            return sb;
        }
    }
}
