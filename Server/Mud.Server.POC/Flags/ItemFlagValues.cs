using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.POC.Flags;

[FlagValues(typeof(IFlagValues), typeof(IItemFlags)), Shared]
public class ItemFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "StayDeath",
        "RandomStats"
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
