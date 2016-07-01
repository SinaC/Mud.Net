using System;
using System.Text.RegularExpressions;

namespace Mud.Server.Helpers
{
    public static class StringExtensions
    {
        private static readonly Regex ColorTagRegex = new Regex(@"\%(\w)\%", RegexOptions.Compiled);

        public static int LengthNoColor(this string s)
        {
            string output = ColorTagRegex.Replace(s, match => String.Empty);
            return output.Length;
        }
    }
}
