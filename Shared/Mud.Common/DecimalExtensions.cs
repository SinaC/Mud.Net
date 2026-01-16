namespace Mud.Common;

public static class DecimalExtensions
{
    public static decimal Lerp(decimal from, decimal to, decimal current, decimal delta)
        => from + current * (to - from) / delta;
}
