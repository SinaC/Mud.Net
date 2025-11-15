using Mud.Logger;

namespace Mud.Domain.Extensions;

public static class AutoFlagsExtensions
{
    public static string PrettyPrint(this AutoFlags flag)
    {
        switch (flag)
        {
            case AutoFlags.Assist: return "autoassist";
            case AutoFlags.Exit: return "autoexit";
            case AutoFlags.Sacrifice: return "autosacrifice";
            case AutoFlags.Gold: return "autogold";
            case AutoFlags.Split: return "autosplit";
            case AutoFlags.Loot: return "autoloot";
            default:
                Log.Default.WriteLine(LogLevels.Error, "PrettyPrint: Invalid AutoFlags {0}", flag);
                return flag.ToString();
        }
    }
}
