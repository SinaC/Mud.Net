using Microsoft.Extensions.Logging;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    public class Rom24AssistFlagValues : FlagValuesBase<string>, IAssistFlagValues
    {
        public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
        {
            "AreaAttack",
            "Backstab",
            "Bash",
            "Berserk",
            "Disarm",
            "Dodge",
            "Fade",
            "Fast",
            "Kick",
            "DirtKick",
            "Parry",
            "Rescue",
            "Tail",
            "Trip",
            "Crush",
            "Bite",
        };

        protected override HashSet<string> HashSet => Flags;

        public Rom24AssistFlagValues(ILogger<Rom24AssistFlagValues> logger)
        : base(logger)
        {

        }
    }
}
