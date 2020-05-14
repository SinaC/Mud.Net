using System;

namespace Mud.Server.Common
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
    }
}
