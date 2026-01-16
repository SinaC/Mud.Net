namespace Mud.Common;

public static class IntExtensions
{
    public static int Lerp(int from, int to, int current, int delta)
        => from + current * (to - from) / delta;
}
