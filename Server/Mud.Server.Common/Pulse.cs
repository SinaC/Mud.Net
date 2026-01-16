namespace Mud.Server.Common;

public class Pulse
{
    private const int PulsePerSeconds = 4;
    private const int PulsePerMinutes = PulsePerSeconds * 60;

    public const int PulseViolence = 3 * PulsePerSeconds;

    public static readonly TimeSpan Infinite = TimeSpan.MaxValue;

    public static int FromSeconds(int seconds)
        => seconds == -1
            ? -1
            : seconds * PulsePerSeconds;

    public static decimal ToSeconds(int pulse)
        => (decimal)PulsePerSeconds / pulse;

    public static int FromMinutes(int minutes)
        => minutes == -1
            ? -1
            : minutes * PulsePerMinutes;
    public static decimal ToMinutes(int pulse)
        => (decimal)PulsePerMinutes / pulse;

    public static int FromTimeSpan(TimeSpan ts)
        => ts.Equals(Infinite)
            ? -1
            : FromSeconds((int)ts.TotalSeconds);

    public static TimeSpan ToTimeSpan(int pulse)
        => TimeSpan.FromSeconds(pulse / PulsePerSeconds);
}
