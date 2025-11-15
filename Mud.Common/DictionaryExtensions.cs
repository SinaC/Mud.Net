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
}
