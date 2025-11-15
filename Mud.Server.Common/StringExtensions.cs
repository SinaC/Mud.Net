using System.Text.RegularExpressions;

namespace Mud.Server.Common;

public static class StringExtensions
{
    private static readonly Regex ColorTagRegex = new(@"\%(\w)\%", RegexOptions.Compiled);

    #region Color tags

    // TODO: better tags such as <r> or #r for red

    public static readonly string Reset = "%x%";
    public static readonly string Red = "%r%";
    public static readonly string Green = "%g%";
    public static readonly string Yellow = "%y%";
    public static readonly string Blue = "%b%";
    public static readonly string Magenta = "%m%";
    public static readonly string Cyan = "%c%";
    public static readonly string Gray = "%w%";
    public static readonly string LightRed = "%R%";
    public static readonly string LightGreen = "%G%";
    public static readonly string LightYellow = "%Y%";
    public static readonly string LightBlue = "%B%";
    public static readonly string LightMagenta = "%M%";
    public static readonly string LightCyan = "%C%";
    public static readonly string White = "%W%";

    #endregion

    public static int LengthNoColor(this string s)
    {
        string output = ColorTagRegex.Replace(s, match => string.Empty);
        return output.Length;
    }
}
