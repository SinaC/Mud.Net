using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(ICharacterFlagValues)), Shared]
public class CharacterFlagValues : FlagValuesBase<string>, ICharacterFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
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
    };

    private ILogger<CharacterFlagValues> Logger { get; }

    public CharacterFlagValues(ILogger<CharacterFlagValues> logger)
    {
        Logger = logger;
    }

    protected override HashSet<string> HashSet => Flags;

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
        //        "DetectEvil" => "%r%(R)%x%",
        //        "DetectGood" => "%Y%(G)%x%",
        //        _ => string.Empty, // we don't want to display the other flags
        //    };
        //else
            return flag switch
            {
                "Charm" => "%C%(Charmed)%x%",
                "Flying" => "%c%(Flying)%x%",
                "Invisible" => "%y%(Invis)%x%",
                "Hide" => "%b%(Hide)%x%",
                "Sneak" => "%R%(Sneaking)%x%",
                "PassDoor" => "%c%(Translucent)%x%",
                "FaerieFire" => "%m%(Pink Aura)%x%",
                "DetectEvil" => "%r%(Red Aura)%x%",
                "DetectGood" => "%Y%(Golden Aura)%x%",
                _ => string.Empty, // we don't want to display the other flags
            };
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Character flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
