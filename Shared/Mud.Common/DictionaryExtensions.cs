namespace Mud.Common;

public static class DictionaryExtensions
{
    public static void Increment(this Dictionary<int, int> dictionary, int key)
    {
        if (dictionary.TryGetValue(key, out int value))
            dictionary[key] = ++value;
        else
            dictionary.Add(key, 1);
    }

    public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey: notnull
        where TValue: class
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;
        return null;
    }
}
