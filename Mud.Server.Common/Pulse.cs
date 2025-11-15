namespace Mud.Server.Common;

public class Pulse
{
    public const int PulsePerSeconds = 4;
    public const int PulsePerMinutes = PulsePerSeconds * 60;
    public const int PulseDelay = 1000 / PulsePerSeconds;
    public const int PulseViolence = 3 * PulsePerSeconds;

    public static readonly TimeSpan Infinite = TimeSpan.MaxValue;

    public static int FromSeconds(int seconds) => seconds == -1
        ? -1
        : seconds * PulsePerSeconds;

    public static int FromMinutes(int minutes) => minutes == -1
        ? -1
        : minutes * PulsePerMinutes;

    public static int FromTimeSpan(TimeSpan ts) => ts.Equals(Infinite)
        ? -1
        : FromSeconds((int)ts.TotalSeconds);

    public static TimeSpan FromPulse(int pulse) => TimeSpan.FromSeconds(pulse / PulsePerSeconds);
}
