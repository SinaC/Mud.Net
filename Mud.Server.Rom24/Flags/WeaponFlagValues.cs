using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IWeaponFlagValues)), Shared]
public class WeaponFlagValues : FlagValuesBase<string>, IWeaponFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Flaming",
        "Frost",
        "Vampiric",
        "Sharp",
        "Vorpal",
        "TwoHands",
        "Shocking",
        "Poison",
    };

    protected override HashSet<string> HashSet => Flags;

    public WeaponFlagValues(ILogger<WeaponFlagValues> logger)
        : base(logger)
    {
    }

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

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Weapon flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
