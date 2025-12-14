using Mud.Common.Attributes;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

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
            "Flaming" => "%R%(Flaming)%x%",
            "Frost" => "%C%(Frost)%x%",
            "Vampiric" => "%D%(Vampiric)%x%",
            "Sharp" => "%W%(Sharp)%x%",
            "Vorpal" => "%M%(Vorpal)%x%",
            "TwoHands" => "%W%(Two-handed)%x%",
            "Shocking" => "%Y%(Sparkling)%x%",
            "Poison" => "%G%(Envenomed)%x%",
            _ => flag.ToString(),
        };
    }
}
