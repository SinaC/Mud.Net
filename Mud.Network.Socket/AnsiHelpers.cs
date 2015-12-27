using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mud.Network.Socket
{
    // Convert %^ColorName%^ into CSI ansi code matching specified ColorName
    public static class AnsiHelpers
    {
        //https://github.com/SyntaxColoring/ansitext/blob/master/source/ansitext.d
        private static readonly string CSI = ASCIIEncoding.ASCII.GetString(new byte[] {27}) + "[{0}m";
        private static readonly Regex Regex = new Regex(@"\%\^(\w+)\%\^", RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<string, string> ColorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"reset", String.Format(CSI, 0)},
            {"bold", String.Format(CSI, 1)},
            {"italics", String.Format(CSI, 3)},
            {"underline", String.Format(CSI, 4)},
            {"nobold", String.Format(CSI, 22)},
            {"black", String.Format(CSI, 30)},
            {"red", String.Format(CSI, 31)},
            {"green", String.Format(CSI, 32)},
            {"yellow", String.Format(CSI, 33)},
            {"blue", String.Format(CSI, 34)},
            {"magenta", String.Format(CSI, 35)},
            {"cyan", String.Format(CSI, 36)},
            {"white", String.Format(CSI, 37)},
            {"default", String.Format(CSI, 39)},
            {"blackback", String.Format(CSI, 40)},
            {"redback", String.Format(CSI, 41)},
            {"greenback", String.Format(CSI, 42)},
            {"yellowback", String.Format(CSI, 43)},
            {"blueback", String.Format(CSI, 44)},
            {"magentaback", String.Format(CSI, 45)},
            {"cyanback", String.Format(CSI, 46)},
            {"whiteback", String.Format(CSI, 47)},
            {"defaultback", String.Format(CSI, 49)},
        };

        public static string Colorize(string input)
        {
            //http://stackoverflow.com/questions/1231768/c-sharp-string-replace-with-dictionary
            string output = Regex.Replace(input, match => ColorMap[match.Groups[1].Value]);
            return output;
        }
    }
}
