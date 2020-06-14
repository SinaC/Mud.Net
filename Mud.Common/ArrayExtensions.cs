namespace Mud.Common
{
    public static class ArrayExtensions
    {
        public static T Get<T>(this T[] array, int index) => array[index.Range(array)];
    }
}
