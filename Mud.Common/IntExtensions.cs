using System;

namespace Mud.Common
{
    public static class IntExtensions
    {
        public static int Range(this int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static int Range(this int value, Array array)
        {
            return Math.Max(0, Math.Min(value, array.Length)-1);
        }

        public static int Lerp(int from, int to, int current, int delta) => from + current * (to - from) / delta;

        public static string FormatDelay(this int delayInSeconds)
        {
            if (delayInSeconds < 60)
                return delayInSeconds + " second" + (delayInSeconds != 1 ? "s" : string.Empty);
            int minutes = (delayInSeconds + 60 - 1) / 60; // -> ceil(x/60)
            if (minutes < 60)
                return minutes + " minute" + (minutes != 1 ? "s" : string.Empty);
            int hours = (minutes + 60 - 1) / 60; // -> ceil(x/60)
            if (hours < 24)
                return hours + " hour" + (hours != 1 ? "s" : string.Empty);
            int days = (hours + 24 - 1) / 24;
            return days + " day" + (days != 1 ? "s" : string.Empty);
        }

        public static string FormatDelayShort(this int delayInSeconds)
        {
            if (delayInSeconds < 60)
                return delayInSeconds + " sec";
            int minutes = (delayInSeconds + 60 - 1) / 60; // -> ceil(x/60)
            if (minutes < 60)
                return minutes + " min";
            int hours = (minutes + 60 - 1) / 60; // -> ceil(x/60)
            if (hours < 24)
                return hours + " hour" + (hours != 1 ? "s" : string.Empty);
            int days = (hours + 24 - 1) / 24;
            return days + " day" + (days != 1 ? "s" : string.Empty);
        }
    }
}
