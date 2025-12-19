namespace Mud.Common;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> source, Func<T, object> propertySelector)
        => source.GroupBy(propertySelector).Select(x => x.First());

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Func<int, int, int> randomNextFunc)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(randomNextFunc);

        return source.ShuffleIterator(randomNextFunc);
    }

    private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Func<int, int, int> randomNextFunc)
    {
        var buffer = source.ToList();
        for (int i = 0; i < buffer.Count; i++)
        {
            int j = randomNextFunc(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }
}
