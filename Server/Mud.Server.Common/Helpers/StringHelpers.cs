using Mud.Blueprints.Item;
using Mud.Domain;

namespace Mud.Server.Common.Helpers;

public static class StringHelpers
{
    public const string NotYetImplemented = "NOT YET IMPLEMENTED!!";
    public const string NotFound = "Not found.";
    public const string CharacterNotFound = "They aren't here.";
    public const string ItemNotFound = "You do not see that here.";
    public const string ItemInventoryNotFound = "You do not have that item.";
    public const string QuestPrefix = "%R%(QUEST)%x%";
    public const string PagingInstructions = "[Paging : (Enter), (N)ext, (P)revious, (Q)uit, (A)ll]";
    public const string CantFindIt = "You can't find it.";
    public const string SomethingGoesWrong = "Something goes wrong.";
    public const string YouLostYourConcentration = "You lost your concentration.";
    public const string CantDoThatHere = "You can't do that here.";

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

    public static string ItemType(this ItemBlueprintBase blueprint)
        => blueprint.GetType().Name.Replace("Item", string.Empty).Replace("Blueprint", string.Empty);

    public static string DifficultyColor(int characterLevel, int targetLevel)
    {
        var levelDiff = targetLevel - characterLevel;
        if (levelDiff > 5)
            return LightRed;
        else if (levelDiff > 2 && levelDiff <= 5)
            return Red;
        else if (levelDiff >= -2 && levelDiff <= 2)
            return Yellow;
        else if (levelDiff >= -5 && levelDiff < -2)
            return Green;
        else if (levelDiff < -5)
            return Gray;
        return White;
    }


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
}
