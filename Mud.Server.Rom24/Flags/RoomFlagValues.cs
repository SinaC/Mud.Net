using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IRoomFlags)), Shared]
public class RoomFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Dark",
        "NoMob",
        "Indoors",
        "NoScan",
        "Private",
        "Safe",
        "Solitary",
        "PetShop",
        "NoRecall",
        "ImpOnly",
        "GodsOnly",
        "HeroesOnly",
        "NewbiesOnly",
        "Law",
        "NoWhere",
    ];
    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
