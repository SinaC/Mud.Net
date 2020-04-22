using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Mud.Logger;

namespace Mud.Network.Telnet
{
    // Convert %x% into ansi color
    public static class AnsiHelpers
    {
        //https://github.com/SyntaxColoring/ansitext/blob/master/source/ansitext.d
        private static readonly string ResetColorTag = Encoding.ASCII.GetString(new byte[] { 27 }) + "[0m";
        private static readonly string NormalColorTagFormat = Encoding.ASCII.GetString(new byte[] {27}) + "[0;{0}m";
        private static readonly string LightColorTagFormat = Encoding.ASCII.GetString(new byte[] { 27 }) + "[1;{0}m";
        // Will replace %x% using ColorMap searching for x and replacing with Ansi color -> strongly related to Color tags found in Server.Common
        private static readonly Regex Regex = new Regex(@"\%(\w)\%", RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<string, string> ColorMap = new Dictionary<string, string>(StringComparer.Ordinal)
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

            //{"bold", String.Format(NormalColorTag, 1)},
            //{"italics", String.Format(NormalColorTag, 3)},
            //{"underline", String.Format(NormalColorTag, 4)},
            //{"nobold", String.Format(NormalColorTag, 22)},
            //{"black", String.Format(NormalColorTag, 30)},
            //{"default", String.Format(NormalColorTag, 39)},
            //{"blackback", String.Format(NormalColorTag, 40)},
            //{"redback", String.Format(NormalColorTag, 41)},
            //{"greenback", String.Format(NormalColorTag, 42)},
            //{"yellowback", String.Format(NormalColorTag, 43)},
            //{"blueback", String.Format(NormalColorTag, 44)},
            //{"magentaback", String.Format(NormalColorTag, 45)},
            //{"cyanback", String.Format(NormalColorTag, 46)},
            //{"whiteback", String.Format(NormalColorTag, 47)},
            //{"defaultback", String.Format(NormalColorTag, 49)},
        };

        public static string Colorize(string input)
        {
            //http://stackoverflow.com/questions/1231768/c-sharp-string-replace-with-dictionary
            string output = Regex.Replace(input, match =>
            {
                string key = match.Groups[1].Value;
                string mapped;
                if (!ColorMap.TryGetValue(key, out mapped))
                {
                    Log.Default.WriteLine(LogLevels.Error, "Unknown color code [{0}] in {1}", key, input);
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
}
