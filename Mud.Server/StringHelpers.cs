﻿using System;
using System.Collections.Generic;

namespace Mud.Server
{
    public static class StringHelpers
    {
        public static string CharacterNotFound = "They aren't here.";

        #region Color tags

        // TODO: better tags such as <r> or #r for red

        public static string Reset = "%x%";
        public static string Red = "%r%";
        public static string Green = "%g%";
        public static string Yellow = "%y%";
        public static string Blue = "%b%";
        public static string Magenta = "%m%";
        public static string Cyan = "%c%";
        public static string White = "%w%";
        public static string LightRed = "%R%";
        public static string LightGreen = "%G%";
        public static string LightYellow = "%Y%";
        public static string LightBlue = "%B%";
        public static string LightMagenta = "%M%";
        public static string LightCyan = "%C%";
        public static string Grey = "%W%";

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