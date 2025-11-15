using Mud.DataStructures.Flags;
using Mud.Logger;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

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

    public override string PrettyPrint(string flag, bool shortDisplay)
        => flag switch
        {
            "Flaming" => "%R%(Flaming)%x%",
            "Frost" => "%C%(Frost)%x%",
            "Vampiric" => "%D%(Vampiric)%x%",
            "Sharp" => "%W%(Sharp)%x%",
            "Vorpal" => "%M%(Vorpal)%x%",
            "TwoHands" => "%W%(Two-handed)%x%",
            "Shocking" => "%Y%(Sparkling)%x%",
            "Poison" => "%G%(Envenomed)%x%",
            _ => base.PrettyPrint(flag, shortDisplay),
        };

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Log.Default.WriteLine(LogLevels.Error, $"Weapon flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
