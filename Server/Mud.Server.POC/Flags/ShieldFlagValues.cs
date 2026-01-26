using Mud.Common;
using Mud.Common.Attributes;
using Mud.Flags.Attributes;
using Mud.Flags.Interfaces;

namespace Mud.Server.POC.Flags;

[FlagValues(typeof(IFlagValues), typeof(IShieldFlags)), Shared]
public class ShieldFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "FireShield",
        "IceShield",
        "ShockShield"
    ];

    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
    {
        //if (shortDisplay)
        //else
        return flag switch
        {
            string f when StringCompareHelpers.StringEquals(f, "FireShield") => "%R%(Fire)%x%",
            string f when StringCompareHelpers.StringEquals(f, "IceShield") => "%C%(Ice)%x%",
            string f when StringCompareHelpers.StringEquals(f, "ShockShield") => "%Y%(Shock)%x%",
            _ => string.Empty,// we don't want to display the other flags
        };
    }
}
