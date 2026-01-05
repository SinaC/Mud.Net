using System.Text;
using System.Text.RegularExpressions;

namespace Mud.Network.Telnet;

// Convert %x% into ansi color
public static class AnsiHelpers
{
    //https://github.com/SyntaxColoring/ansitext/blob/master/source/ansitext.d
    private static readonly string ResetColorTag = Encoding.ASCII.GetString([27]) + "[0m";
    private static readonly string NormalColorTagFormat = Encoding.ASCII.GetString([27]) + "[0;{0}m";
    private static readonly string LightColorTagFormat = Encoding.ASCII.GetString([27]) + "[1;{0}m";
    // Will replace %x% using ColorMap searching for x and replacing with Ansi color -> strongly related to Color tags found in Mud.Server.Common
    private static readonly Regex Regex = new(@"\%(\w)\%", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> ColorMap = new(StringComparer.Ordinal)
    {
        {"x", ResetColorTag},

        {"r", string.Format(NormalColorTagFormat, 31)},
        {"g", string.Format(NormalColorTagFormat, 32)},
        {"y", string.Format(NormalColorTagFormat, 33)},
        {"b", string.Format(NormalColorTagFormat, 34)},
        {"m", string.Format(NormalColorTagFormat, 35)},
        {"c", string.Format(NormalColorTagFormat, 36)},
        {"w", string.Format(NormalColorTagFormat, 37)},

        {"R", string.Format(LightColorTagFormat, 31)},
        {"G", string.Format(LightColorTagFormat, 32)},
        {"Y", string.Format(LightColorTagFormat, 33)},
        {"B", string.Format(LightColorTagFormat, 34)},
        {"M", string.Format(LightColorTagFormat, 35)},
        {"C", string.Format(LightColorTagFormat, 36)},
        {"W", string.Format(LightColorTagFormat, 37)},

        {"D", string.Format(LightColorTagFormat, 30)},
// https://gist.github.com/JBlond/2fea43a3049b38287e5e9cefc87b2124
    };

    public static string Colorize(string input)
    {
        //http://stackoverflow.com/questions/1231768/c-sharp-string-replace-with-dictionary
        string output = Regex.Replace(input, match =>
        {
            string key = match.Groups[1].Value;
            if (!ColorMap.TryGetValue(key, out var mapped))
            {
                mapped = string.Empty;
            }
            return mapped;
        });
        return output;
    }

    public static string StripColor(string input)
    {
        string output = Regex.Replace(input, match => string.Empty);
        return output;
    }
}
