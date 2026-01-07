using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IShieldFlags)), Shared]
public class ShieldFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Sanctuary",
        "ProtectEvil",
        "ProtectGood"
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
    {
        //if (shortDisplay)
        //    return flag switch
        //    {
        //        "Sanctuary" => "%W%(W)%x%",
        //        _ => string.Empty, // we don't want to display the other flags
        //    };
        //else
        return flag switch
        {
            string f when StringCompareHelpers.StringEquals(f, "Sanctuary") => "%W%(White Aura)%x%",
            _ => string.Empty, // we don't want to display the other flags
        };
    }
}
