using System;
using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Helpers
{
    public static class StringHelpers
    {
        public static string NotYetImplemented = "NOT YET IMPLEMENTED!!" + Environment.NewLine;
        public static string NotFound = "No found." + Environment.NewLine;
        public static string CharacterNotFound = "They aren't here." + Environment.NewLine;
        public static string ItemNotFound = "You do not see that here." + Environment.NewLine;
        public static string ItemInventoryNotFound = "You do not have that item." + Environment.NewLine;

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

        //https://genderneutralpronoun.wordpress.com/tag/ze-and-zir/
        public static readonly IDictionary<Sex, string> Subjects = new Dictionary<Sex, string>
        {
            {Sex.Neutral, "it"},
            {Sex.Male, "he"},
            {Sex.Female, "she"},
        };

        public static readonly IDictionary<Sex, string> Objectives = new Dictionary<Sex, string>
        {
            {Sex.Neutral, "it"},
            {Sex.Male, "him"},
            {Sex.Female, "her"},
        };

        public static readonly IDictionary<Sex, string> Possessives = new Dictionary<Sex, string>
        {
            {Sex.Neutral, "its"},
            {Sex.Male, "his"},
            {Sex.Female, "her"},
        };

        // TODO: other flavor messages (see fight.C:4429)
        public static string DamagePhraseSelf(int damage)
        {
            if (damage == 0) return "miss";
            if (damage <= 4) return "scratch";
            if (damage <= 8) return "graze";
            if (damage <= 12) return "hit";
            if (damage <= 16) return "injure";
            if (damage <= 20) return "wound";
            if (damage <= 24) return "maul";
            if (damage <= 28) return "decimate";
            if (damage <= 32) return "devastate";
            if (damage <= 36) return "maim";
            if (damage <= 40) return "%b%MUTILATE%x%";
            if (damage <= 44) return "%w%DISEMBOWEL%x%";
            if (damage <= 48) return "%r%DISMEMBER%x%";
            if (damage <= 52) return "%m%MASSACRE%x%";
            if (damage <= 56) return "%g%MANGLE%x%";
            if (damage <= 60) return "%c%*** DEMOLISH ***%x%";
            if (damage <= 75) return "%w%**** %m%DEVASTATE%w% ****%x%";
            if (damage <= 100) return "%b%=== %c%OBLITERATE%b% ===%x%";
            if (damage <= 150) return "%g%>>> %y%ANNIHILATE%g% <<<%x%";
            if (damage <= 200) return "%r%-=<*>=- %b%ERADICATE%r% -=<*>=-%x%";
            if (damage <= 300) return "%r%<*>%y%<*>%g%<*>%b%<*>%y% NUKE %b%<*>%g%<*>%y%<*>%r%<*>%x%";
            if (damage <= 400) return "%b%--*-- --*-- RUPTURE --*-- --*--%x%";
            return "do %b%U%r%N%g%S%y%P%c%E%m%A%r%K%b%A%g%B%y%L%c%E %x%things to";
        }

        public static string DamagePhraseOther(int damage)
        {
            if (damage == 0) return "misses";
            if (damage <= 4) return "scratches";
            if (damage <= 8) return "grazes";
            if (damage <= 12) return "hits";
            if (damage <= 16) return "injures";
            if (damage <= 20) return "wounds";
            if (damage <= 24) return "mauls";
            if (damage <= 28) return "decimates";
            if (damage <= 32) return "devastates";
            if (damage <= 36) return "maims";
            if (damage <= 40) return "%b%MUTILATES%x%";
            if (damage <= 44) return "%w%DISEMBOWELS%x%";
            if (damage <= 48) return "%r%DISMEMBERS%x%";
            if (damage <= 52) return "%m%MASSACRES%x%";
            if (damage <= 56) return "%g%MANGLES%x%";
            if (damage <= 60) return "%c%*** DEMOLISHES ***%x%";
            if (damage <= 75) return "%w%**** %m%DEVASTATES%w% ****%x%";
            if (damage <= 100) return "%b%=== %c%OBLITERATES%b% ===%x%";
            if (damage <= 150) return "%g%>>> %y%ANNIHILATES%g% <<<%x%";
            if (damage <= 200) return "%r%-=<*>=- %b%ERADICATES%r% -=<*>=-%x%";
            if (damage <= 300) return "%r%<*>%y%<*>%g%<*>%b%<*>%y% NUKES %b%<*>%g%<*>%y%<*>%r%<*>%x%";
            if (damage <= 400) return "%b%--*-- --*-- RUPTURES --*-- --*--%x%";
            return "does %b%U%r%N%g%S%y%P%c%E%m%A%r%K%b%A%g%B%y%L%c%E %x%things to";
        }

        public static string UpperFirstLetter(string text, string ifNull = "???")
        {
            if (text == null)
                return ifNull;

            if (text.Length > 1)
                return Char.ToUpper(text[0]) + text.Substring(1);

            return text.ToUpper();
        }

        public static string FormatDelay(int delay)
        {
            if (delay < 60)
                return delay + " second" + (delay != 1 ? "s" : String.Empty);
            int minutes = (delay + 60 - 1)/60; // -> ceil(x/60)
            if (minutes < 60)
                return minutes + " minute" + (minutes != 1 ? "s" : String.Empty);
            int hours = (minutes + 60 - 1)/60; // -> ceil(x/60)
            if (hours < 24)
                return hours + " hour" + (hours != 1 ? "s" : String.Empty);
            int days = (hours + 24 - 1)/24;
            return days + " day" + (days != 1 ? "s" : String.Empty);
        }

        public static string CenterText(string text, int length)
        {
            if (text.Length >= length)
                return text;
            int space = length - text.Length;
            int left = space/2;
            //int right = space/2 + (space%2);
            return text.PadLeft(left + text.Length).PadRight(length);
        }
    }
}
