namespace Mud.Common;

public static class TimeSpanExtensions
{
    public static string FormatDelay(this TimeSpan ts)
    {
        int seconds = (int)ts.TotalSeconds;
        if (seconds < 60)
            return seconds + " second" + (seconds != 1 ? "s" : string.Empty);
        int minutes = (int)ts.TotalMinutes;
        if (minutes < 60)
            return minutes + " minute" + (minutes != 1 ? "s" : string.Empty);
        int hours = (int)ts.TotalHours;
        if (hours < 24)
            return hours + " hour" + (hours != 1 ? "s" : string.Empty);
        int days = (int)ts.TotalDays;
        return days + " day" + (days != 1 ? "s" : string.Empty);
    }

    public static string FormatDelayShort(this TimeSpan ts)
    {
        int seconds = (int)ts.TotalSeconds;
        if (seconds < 60)
            return seconds + " sec";
        int minutes = (int)ts.TotalMinutes;
        if (minutes < 60)
            return minutes + " min";
        int hours = (int)ts.TotalHours;
        if (hours < 24)
            return hours + " hour" + (hours != 1 ? "s" : string.Empty);
        int days = (int)ts.TotalDays;
        return days + " day" + (days != 1 ? "s" : string.Empty);

    }
}
