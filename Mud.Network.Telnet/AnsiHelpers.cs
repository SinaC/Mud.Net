using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Mud.Logger;

namespace Mud.Network.Telnet
{
    // Convert %c% into ansi color
    public static class AnsiHelpers
    {
        //https://github.com/SyntaxColoring/ansitext/blob/master/source/ansitext.d
        private static readonly string ResetColorTag = ASCIIEncoding.ASCII.GetString(new byte[] { 27 }) + "[0m";
        private static readonly string NormalColorTag = ASCIIEncoding.ASCII.GetString(new byte[] {27}) + "[0;{0}m";
        private static readonly string LightColorTag = ASCIIEncoding.ASCII.GetString(new byte[] { 27 }) + "[1;{0}m";
        private static readonly Regex Regex = new Regex(@"\%(\w)\%", RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<string, string> ColorMap = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            {"x", ResetColorTag},

            {"r", String.Format(NormalColorTag, 31)},
            {"g", String.Format(NormalColorTag, 32)},
            {"y", String.Format(NormalColorTag, 33)},
            {"b", String.Format(NormalColorTag, 34)},
            {"m", String.Format(NormalColorTag, 35)},
            {"c", String.Format(NormalColorTag, 36)},
            {"w", String.Format(NormalColorTag, 37)},

            {"R", String.Format(LightColorTag, 31)},
            {"G", String.Format(LightColorTag, 32)},
            {"Y", String.Format(LightColorTag, 33)},
            {"B", String.Format(LightColorTag, 34)},
            {"M", String.Format(LightColorTag, 35)},
            {"C", String.Format(LightColorTag, 36)},
            {"W", String.Format(LightColorTag, 37)},

            {"D", String.Format(LightColorTag, 30)},

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
                    mapped = String.Empty;
                }
                return mapped;
            });
            return output;
        }

        public static string StripColor(string input)
        {
            string output = Regex.Replace(input, match => String.Empty);
            return output;
        }
    }
}
