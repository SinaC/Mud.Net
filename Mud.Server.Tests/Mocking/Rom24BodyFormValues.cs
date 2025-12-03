using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    public class Rom24BodyFormValues : FlagValuesBase<string>, IBodyFormValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
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

        public Rom24BodyFormValues(ILogger<Rom24BodyFormValues> logger)
        : base(logger)
        {

        }
    }
}
