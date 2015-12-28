using System;
using System.Collections.Generic;

namespace Mud.Server
{
    public static class StringHelpers
    {
        public static string CharacterNotFound = "They aren't here.";

        #region Color tags

        // TODO: better tags such as <r> or #r for red

        public static string Reset = "%^reset%^";
        public static string Bold = "%^bold%^";
        public static string Italics = "%^italics%^";
        public static string Underline = "%^underline%^";
        public static string NoBold = "%^nobold%^";
        public static string Black = "%^black%^";
        public static string Red = "%^red%^";
        public static string Green = "%^green%^";
        public static string Yellow = "%^yellow%^";
        public static string Blue = "%^blue%^";
        public static string Magenta = "%^magenta%^";
        public static string Cyan = "%^cyan%^";
        public static string White = "%^white%^";
        public static string Default = "%^default%^";
        public static string BlackBack = "%^blackback%^";
        public static string RedBack = "%^redback%^";
        public static string GreenBack = "%^greenback%^";
        public static string YellowBack = "%^yellowback%^";
        public static string BlueBack = "%^blueback%^";
        public static string MagentaBack = "%^magentaback%^";
        public static string CyanBack = "%^cyanback%^";
        public static string WhiteBack = "%^whiteback%^";
        public static string DefaultBack = "%^defaultback%^";

        #endregion

        public static string UpperFirstLetter(string text, string ifNull = "???")
        {
            if (text == null)
                return ifNull;

            if (text.Length > 1)
                return Char.ToUpper(text[0]) + text.Substring(1);

            return text.ToUpper();
        }

        //https://genderneutralpronoun.wordpress.com/tag/ze-and-zir/
        public static readonly IDictionary<Sex, string> Subjects = new Dictionary<Sex, string>
        {
            { Sex.Neutral, "it" },
            { Sex.Male, "he"},
            { Sex.Female, "she"},
        };

        public static readonly IDictionary<Sex, string> Objectives = new Dictionary<Sex, string>
        {
            { Sex.Neutral, "it" },
            { Sex.Male, "him"},
            { Sex.Female, "her"},
        };

        public static readonly IDictionary<Sex, string> Possessives = new Dictionary<Sex, string>
        {
            { Sex.Neutral, "its" },
            { Sex.Male, "his"},
            { Sex.Female, "her"},
        };
    }
}
