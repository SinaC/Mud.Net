using Mud.Common.Attributes;
using Mud.Flags;
using Mud.Flags.Interfaces;

namespace Mud.Server.POC.Flags;

[FlagValues(typeof(IFlagValues), typeof(IItemFlags)), Shared]
public class ActFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "NoCorpse", // when killed, will not create a corpse -> inventory/equipments will be dropped on the floor
        "NoLootOnDeath" // when killed, destroy its inventory/equipments
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
