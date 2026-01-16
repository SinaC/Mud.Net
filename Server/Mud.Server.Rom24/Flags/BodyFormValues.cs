using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IBodyForms)), Shared]
public class BodyFormValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Edible",
        "Poison",
        "Magical",
        "InstantDecay",
        "Other", // defined by material
        "Animal",
        "Sentient",
        "Undead",
        "Construct",
        "Mist",
        "Intangible",
        "Biped",
        "Centaur",
        "Insect",
        "Spider",
        "Crustacean",
        "Worm",
        "Blob",
        "Mammal",
        "Bird",
        "Reptile",
        "Snake",
        "Dragon",
        "Amphibian",
        "Fish",
        "ColdBlood",
        "Fur",
        "FourArms",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
