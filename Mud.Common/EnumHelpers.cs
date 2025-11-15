namespace Mud.Common;

public static class EnumHelpers
{
    public static IEnumerable<T> GetValues<T>()
        where T : Enum
        => Enum.GetValues(typeof(T)).Cast<T>();

    public static bool TryFindByName<T>(string name, out T value)
        where T : struct, Enum
        => Enum.TryParse(name, true, out value);

    public static bool TryFindByPrefix<T>(string prefix, out T value, params T[] excluded)
        where T : struct, Enum
    {
        IEnumerable<string> names = excluded?.Length > 0 
            ? Enum.GetValues(typeof(T)).Cast<T>().Except(excluded).Select(x => x.ToString())
            : Enum.GetNames(typeof(T));
        string? name = names.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x, prefix));
        if (!string.IsNullOrWhiteSpace(name))
            return Enum.TryParse(name, true, out value);
        value = default;
        return false;
    }

    public static int GetCount<T>()
        where T : Enum
        => Enum.GetValues(typeof(T)).Length;
}
