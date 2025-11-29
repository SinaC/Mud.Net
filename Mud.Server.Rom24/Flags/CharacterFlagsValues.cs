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

    public CharacterFlagValues(ILogger<CharacterFlagValues> logger)
        : base(logger)
    {
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Character flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
