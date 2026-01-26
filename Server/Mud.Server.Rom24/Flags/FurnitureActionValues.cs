using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IFurnitureActions)), Shared]
public class FurnitureActionValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Stand",
        "Sit",
        "Rest",
        "Sleep",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
