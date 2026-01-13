using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.POC.Flags;

[FlagValues(typeof(IFlagValues), typeof(IItemFlags)), Shared]
public class ActFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "NoCorpse", // when killed, will not create a corpse (if NoCorpse is set and DropItemsOnDeath is not set, inventory/equipments/... will be destroyed)
        "DropItemsOnDeath" // when killed, inventory/equipments/... will be dropped on the floor instead of corpse
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
