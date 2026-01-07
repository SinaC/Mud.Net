using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(ICharacterFlags)), Shared]
public class CharacterFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Blind",
        "Invisible",
        "DetectEvil",
        "DetectInvis",
        "DetectMagic",
        "DetectHidden",
        "DetectGood",
        "FaerieFire",
        "Infrared",
        "Curse",
        "Poison",
        "Sneak",
        "Hide",
        "Sleep",
        "Charm",
        "Flying",
        "PassDoor",
        "Haste",
        "Calm",
        "Plague",
        "Weaken",
        "DarkVision",
        "Berserk",
        "Swim",
        "Regeneration",
        "Slow",
    ];
    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
    {
        //if (shortDisplay)
        //    return flag switch
        //    {
        //        "Charm" => "%C%(C)%x%",
        //        "Flying" => "%c%(F)%x%",
        //        "Invisible" => "%y%(I)%x%",
        //        "Hide" => "%b%(H)%x%",
        //        "Sneak" => "%R%(S)%x%",
        //        "PassDoor" => "%c%(T)%x%",
        //        "FaerieFire" => "%m%(P)%x%",
        //        _ => string.Empty, // we don't want to display the other flags
        //    };
        //else
        return flag switch
        {
            string f when StringCompareHelpers.StringEquals(f, "Charm") => "%C%(Charmed)%x%",
            string f when StringCompareHelpers.StringEquals(f, "Flying") => "%c%(Flying)%x%",
            string f when StringCompareHelpers.StringEquals(f, "Invisible") => "%y%(Invis)%x%",
            string f when StringCompareHelpers.StringEquals(f, "Hide") => "%b%(Hide)%x%",
            string f when StringCompareHelpers.StringEquals(f, "Sneak") => "%R%(Sneaking)%x%",
            string f when StringCompareHelpers.StringEquals(f, "PassDoor") => "%c%(Translucent)%x%",
            string f when StringCompareHelpers.StringEquals(f, "FaerieFire") => "%m%(Pink Aura)%x%",
            _ => string.Empty,// we don't want to display the other flags
        };
    }
}
