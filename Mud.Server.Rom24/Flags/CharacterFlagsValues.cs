using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

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
        "Sanctuary",
        "FaerieFire",
        "Infrared",
        "Curse",
        "Poison",
        "ProtectEvil",
        "ProtectGood",
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

    protected override HashSet<string> HashSet => Flags;

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Log.Default.WriteLine(LogLevels.Error, $"Character flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
