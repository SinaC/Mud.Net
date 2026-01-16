namespace Mud.Common;

public static class StringCompareHelpers
{
    public static readonly Func<string, string, bool> StringEquals = (s, s1)
        => string.Equals(s, s1, StringComparison.InvariantCultureIgnoreCase);

    public static readonly Func<string, string, bool> StringStartsWith = (s, s1)
        => s.StartsWith(s1, StringComparison.InvariantCultureIgnoreCase);

    public static readonly Func<IEnumerable<string>, string, bool> AnyStringEquals = (keys, s)
        => keys.Any(x => StringEquals(x, s));

    public static readonly Func<IEnumerable<string>, string, bool> AnyStringStartsWith = (keys, s)
        => keys.Any(x => StringStartsWith(x, s));

    // every item in parameters must be found in keys (full compare)
    public static readonly Func<IEnumerable<string>, IEnumerable<string>, bool> AllStringsEquals = (keys, parameters)
        => parameters.All(x => keys.Any(y => StringEquals(y, x)));

    // every item in parameters must be found in keys (starts with)
    public static readonly Func<IEnumerable<string>, IEnumerable<string>, bool> AllStringsStartsWith = (keys, parameters)
        => parameters.All(x => keys.Any(y => StringStartsWith(y, x)));
}
