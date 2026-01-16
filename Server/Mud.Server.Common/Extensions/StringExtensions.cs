using System.Text.RegularExpressions;

namespace Mud.Server.Common.Extensions;

public static class StringExtensions
{
    private static readonly Regex ColorTagRegex = new(@"\%(\w)\%", RegexOptions.Compiled);

    public static int LengthNoColor(this string s)
    {
        string output = ColorTagRegex.Replace(s, match => string.Empty);
        return output.Length;
    }
}
