using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Flags;

[FlagValues(typeof(IFlagValues), typeof(IWiznetFlags)), Shared]
public class WiznetFlagValues : IFlagValues
{
    private static readonly string[] Flags =
    [
        "Incarnate",
        "Punish",
        "Logins",
        "Deaths",
        "MobDeaths",
        "Levels",
        "Snoops",
        "Bugs",
        "Typos",
        "Help",
        "Load",
        "Promote",
        "Resets",
        "Restore",
        "Immortal",
        "Saccing",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
        => string.Empty; // we don't want to display the flags
}
