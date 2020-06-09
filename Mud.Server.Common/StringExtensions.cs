using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mud.Server.Common
{
    public static class StringExtensions
    {
        private static readonly Regex ColorTagRegex = new Regex(@"\%(\w)\%", RegexOptions.Compiled);

        #region Color tags

        // TODO: better tags such as <r> or #r for red

        public static string Reset = "%x%";
        public static string Red = "%r%";
        public static string Green = "%g%";
        public static string Yellow = "%y%";
        public static string Blue = "%b%";
        public static string Magenta = "%m%";
        public static string Cyan = "%c%";
        public static string Gray = "%w%";
        public static string LightRed = "%R%";
        public static string LightGreen = "%G%";
        public static string LightYellow = "%Y%";
        public static string LightBlue = "%B%";
        public static string LightMagenta = "%M%";
        public static string LightCyan = "%C%";
        public static string White = "%W%";

        #endregion

        public static string AfterLast(this string s, char c) => s.Substring(s.LastIndexOf(c) + 1);

        public static int LengthNoColor(this string s)
        {
            string output = ColorTagRegex.Replace(s, match => string.Empty);
            return output.Length;
        }

        public static string CenterText(this string text, int length)
        {
            if (text.Length >= length)
                return text;
            int space = length - text.Length;
            int left = space / 2;
            //int right = space/2 + (space%2);
            return text.PadLeft(left + text.Length).PadRight(length);
        }

        public static string UpperFirstLetter(this string text, string ifNull = "???")
        {
            if (text == null)
                return ifNull;

            if (text.Length > 1)
                return char.ToUpper(text[0]) + text.Substring(1);

            return text.ToUpper();
        }

        public static string MaxLength(this string input, int length) => input?.Substring(0, Math.Min(length, input.Length));

        public static string ToPascalCase(this string s) => string.Join(" ", s.Split(' ').Select(token => char.ToUpperInvariant(token[0]) + token.Substring(1)));
    }
}
