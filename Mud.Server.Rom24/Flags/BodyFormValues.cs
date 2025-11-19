using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

public class BodyFormValues : FlagValuesBase<string>, IBodyFormValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
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
    };

    protected override HashSet<string> HashSet => Flags;

    public BodyFormValues(ILogger<BodyFormValues> logger)
        : base(logger)
    {
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Body form flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
