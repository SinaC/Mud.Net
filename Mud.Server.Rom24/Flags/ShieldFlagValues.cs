using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Rom24.Flags;

[Export(typeof(IShieldFlagValues)), Shared]
public class ShieldFlagValues : FlagValuesBase<string>, IShieldFlagValues
{
    private static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Sanctuary",
        "ProtectEvil",
        "ProtectGood"
    };

    protected override HashSet<string> HashSet => Flags;

    public ShieldFlagValues(ILogger<ShieldFlagValues> logger)
        : base(logger)
    {
    }

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
                "Sanctuary" => "%W%(White Aura)%x%",
                _ => string.Empty, // we don't want to display the other flags
            };
    }

    public override void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<string> values)
    {
        Logger.LogError("Shield flags '{values}' not found in {type}", string.Join(",", values), GetType().FullName);
    }
}
