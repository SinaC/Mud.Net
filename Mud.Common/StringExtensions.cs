using System;
using System.Linq;

namespace Mud.Common
{
    public static class StringExtensions
    {
        public static string AfterLast(this string s, char c) => s.Substring(s.LastIndexOf(c) + 1);

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

        public static string ToPascalCase(this string s) => string.Join(" ", s.Split(' ').Select(token => token == "of" ? token : char.ToUpperInvariant(token[0]) + token.Substring(1)));

        public static string Quoted(this string s)
            => s == null
            ? null
            : (
                s.Contains(' ')
                    ? $"'{s}'"
                    : s);
    }
}
