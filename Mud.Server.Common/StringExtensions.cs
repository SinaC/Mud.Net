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

        public static int LengthNoColor(this string s)
        {
            string output = ColorTagRegex.Replace(s, match => string.Empty);
            return output.Length;
        }
    }
}
