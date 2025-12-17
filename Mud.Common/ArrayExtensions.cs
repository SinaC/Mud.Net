namespace Mud.Common;

public static class ArrayExtensions
{
    public static T Get<T>(this T[] array, int index)
        => array[Math.Clamp(index, 0, array.Length-1)];
}
