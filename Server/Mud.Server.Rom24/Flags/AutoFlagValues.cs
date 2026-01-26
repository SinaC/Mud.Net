using Mud.Common;
using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IAutoFlags)), Shared]
public class AutoFlagValues : IFlagValues
{
    private static readonly string[] Flags =
    [
        "Assist",
        "Exit",
        "Sacrifice",
        "Gold",
        "Split",
        "Loot",
        "Affect",
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
    {
        return flag switch
        {
            string f when StringCompareHelpers.StringEquals(f, "Affect") => "autoaffect",
            string f when StringCompareHelpers.StringEquals(f, "Assist") => "autoassist",
            string f when StringCompareHelpers.StringEquals(f, "Exit") => "autoexit",
            string f when StringCompareHelpers.StringEquals(f, "Sacrifice") => "autosacrifice",
            string f when StringCompareHelpers.StringEquals(f, "Gold") => "autogold",
            string f when StringCompareHelpers.StringEquals(f, "Split") => "autosplit",
            string f when StringCompareHelpers.StringEquals(f, "Loot") => "autoloot",
            _ => flag.ToString()
        };
    }
}
