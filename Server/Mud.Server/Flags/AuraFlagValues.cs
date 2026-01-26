using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Flags;

[FlagValues(typeof(IFlagValues), typeof(IAuraFlags)), Shared]
public class AuraFlagValues : IFlagValues
{
    private static readonly string[] Flags =
    [
        "StayDeath",
        "NoDispel",
        "Permanent",
        "Hidden",
        "Shapeshift",
        "Inherent",
        "NoSave",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
