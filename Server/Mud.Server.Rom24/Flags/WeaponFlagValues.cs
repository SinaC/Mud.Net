using Mud.Common;
using Mud.Common.Attributes;
using Mud.Flags;
using Mud.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[FlagValues(typeof(IFlagValues), typeof(IWeaponFlags)), Shared]
public class WeaponFlagValues : IFlagValues
{
    private static readonly string[] Flags = 
    [
        "Flaming",
        "Frost",
        "Vampiric",
        "Sharp",
        "Vorpal",
        "TwoHands",
        "Shocking",
        "Poison",
    ];
    public IEnumerable<string> AvailableFlags => Flags;

    public string PrettyPrint(string flag, bool shortDisplay)
    {
        //if (shortDisplay)
        //    return flag switch
        //    {
        //        "Flaming" => "%R%(F)%x%",
        //        "Frost" => "%C%(F)%x%",
        //        "Vampiric" => "%D%(V)%x%",
        //        "Sharp" => "%W%(S)%x%",
        //        "Vorpal" => "%M%(V)%x%",
        //        "TwoHands" => "%W%(T)%x%",
        //        "Shocking" => "%Y%(S)%x%",
        //        "Poison" => "%G%(E)%x%",
        //        _ => flag.ToString(),
        //    };
        //else
        return flag switch
        {
            string f when StringCompareHelpers.StringEquals(f, "Flaming") => "%R%(Flaming)%x%",
             string f when StringCompareHelpers.StringEquals(f, "Frost") => "%C%(Frost)%x%",
             string f when StringCompareHelpers.StringEquals(f, "Vampiric") => "%D%(Vampiric)%x%",
             string f when StringCompareHelpers.StringEquals(f, "Sharp") => "%W%(Sharp)%x%",
             string f when StringCompareHelpers.StringEquals(f, "Vorpal") => "%M%(Vorpal)%x%",
             string f when StringCompareHelpers.StringEquals(f, "TwoHands") => "%W%(Two-handed)%x%",
             string f when StringCompareHelpers.StringEquals(f, "Shocking") => "%Y%(Sparkling)%x%",
             string f when StringCompareHelpers.StringEquals(f, "Poison") => "%G%(Envenomed)%x%",
            _ => flag.ToString(),
        };
    }
}
