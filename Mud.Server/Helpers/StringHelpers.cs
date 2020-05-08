using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Helpers
{
    public static class StringHelpers
    {
        public static string NotYetImplemented = "NOT YET IMPLEMENTED!!";
        public static string NotFound = "Not found.";
        public static string CharacterNotFound = "They aren't here.";
        public static string ItemNotFound = "You do not see that here.";
        public static string ItemInventoryNotFound = "You do not have that item.";
        public static string QuestPrefix = "%R%(QUEST)%x%";
        public static string PagingInstructions = "[Paging : (Enter), (N)ext, (P)revious, (Q)uit, (A)ll]";
        public static string CantFindIt = "You can't find it.";

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

        public static string ResourceColor(ResourceKinds resource)
        {
            switch (resource)
            {
                case ResourceKinds.Mana:
                    return "%B%Mana%x%";
                case ResourceKinds.Psy:
                    return "%y%Psy%x%";
                default:
                    return string.Empty;
            }
        }

        public static string SchoolTypeColor(SchoolTypes schoolType)
        {
            //switch (schoolType)
            //{
            //    case SchoolTypes.Arcane:
            //        return "%B%Arcane%x%";
            //    case SchoolTypes.Fire:
            //        return "%R%Fire%x%";
            //    case SchoolTypes.Frost:
            //        return "%C%Frost%x%";
            //    case SchoolTypes.Holy:
            //        return "%Y%Holy%x%";
            //    case SchoolTypes.Nature:
            //        return "%G%Nature%x%";
            //    case SchoolTypes.Physical:
            //        return "%W%Physical%x%";
            //    case SchoolTypes.Shadow:
            //        return "%M%Shadow%x%";
            //    default:
            //        return "(none)";
            //}
            return "(none)"; // TODO
        }

        public static string ShortExitDirections(ExitDirections exitDirections)
        {
            switch (exitDirections)
            {
                case ExitDirections.North:
                    return "N";
                case ExitDirections.East:
                    return "E";
                case ExitDirections.South:
                    return "S";
                case ExitDirections.West:
                    return "W";
                case ExitDirections.Up:
                    return "U";
                case ExitDirections.Down:
                    return "D";
                case ExitDirections.NorthEast:
                    return "ne";
                case ExitDirections.NorthWest:
                    return "nw";
                case ExitDirections.SouthEast:
                    return "se";
                case ExitDirections.SouthWest:
                    return "sw";
                default:
                    return "?";
            }
        }

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
            if (damage <= 44) return "%W%DISEMBOWEL%x%";
            if (damage <= 48) return "%r%DISMEMBER%x%";
            if (damage <= 52) return "%m%MASSACRE%x%";
            if (damage <= 56) return "%g%MANGLE%x%";
            if (damage <= 60) return "%c%*** DEMOLISH ***%x%";
            if (damage <= 75) return "%W%**** %m%DEVASTATE%W% ****%x%";
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
            if (damage <= 44) return "%W%DISEMBOWELS%x%";
            if (damage <= 48) return "%r%DISMEMBERS%x%";
            if (damage <= 52) return "%m%MASSACRES%x%";
            if (damage <= 56) return "%g%MANGLES%x%";
            if (damage <= 60) return "%c%*** DEMOLISHES ***%x%";
            if (damage <= 75) return "%W%**** %m%DEVASTATES%W% ****%x%";
            if (damage <= 100) return "%b%=== %c%OBLITERATES%b% ===%x%";
            if (damage <= 150) return "%g%>>> %y%ANNIHILATES%g% <<<%x%";
            if (damage <= 200) return "%r%-=<*>=- %b%ERADICATES%r% -=<*>=-%x%";
            if (damage <= 300) return "%r%<*>%y%<*>%g%<*>%b%<*>%y% NUKES %b%<*>%g%<*>%y%<*>%r%<*>%x%";
            if (damage <= 400) return "%b%--*-- --*-- RUPTURES --*-- --*--%x%";
            return "does %b%U%r%N%g%S%y%P%c%E%m%A%r%K%b%A%g%B%y%L%c%E %x%things to";
        }

        public static string FormatDelay(int delayInSeconds)
        {
            if (delayInSeconds < 60)
                return delayInSeconds + " second" + (delayInSeconds != 1 ? "s" : string.Empty);
            int minutes = (delayInSeconds + 60 - 1)/60; // -> ceil(x/60)
            if (minutes < 60)
                return minutes + " minute" + (minutes != 1 ? "s" : string.Empty);
            int hours = (minutes + 60 - 1)/60; // -> ceil(x/60)
            if (hours < 24)
                return hours + " hour" + (hours != 1 ? "s" : string.Empty);
            int days = (hours + 24 - 1)/24;
            return days + " day" + (days != 1 ? "s" : string.Empty);
        }

        public static string FormatDelayShort(int delayInSeconds)
        {
            if (delayInSeconds < 60)
                return delayInSeconds + " sec";
            int minutes = (delayInSeconds + 60 - 1) / 60; // -> ceil(x/60)
            if (minutes < 60)
                return minutes + " min";
            int hours = (minutes + 60 - 1) / 60; // -> ceil(x/60)
            if (hours < 24)
                return hours + " hour" + (hours != 1 ? "s" : string.Empty);
            int days = (hours + 24 - 1) / 24;
            return days + " day" + (days != 1 ? "s" : string.Empty);
        }

        public static string PrettyPrint(this AffectOperators op)
        {
            switch (op)
            {
                case AffectOperators.Add: return "by";
                case AffectOperators.Or: return "by adding";
                case AffectOperators.Assign: return "by setting to";
                case AffectOperators.Nor: return "by removing";
            }
            return "???";
        }
    }
}
