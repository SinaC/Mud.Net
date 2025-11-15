using Mud.Logger;

namespace Mud.Domain.Extensions;

public static class SkyStatesExtensions
{
    public static string PrettyPrint(this SkyStates state)
    {
        switch (state)
        {
            case SkyStates.Cloudless: return "cloudless";
            case SkyStates.Cloudy: return "cloudy";
            case SkyStates.Raining: return "rainy";
            case SkyStates.Lightning: return "lit by flashes of lightning";
            default:
                Log.Default.WriteLine(LogLevels.Error, "SkyStates.PrettyPrint: Invalid sky state {0}", state);
                return state.ToString();
        }
    }
}
