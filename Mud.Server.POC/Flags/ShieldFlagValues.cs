using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

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
                "FireShield" => "%R%(Fire)%x%",
                "IceShield" => "%C%(Ice)%x%",
                "ShockShield" => "%Y%(Shock)%x%",
                _ => string.Empty, // we don't want to display the other flags
            };
    }
}
