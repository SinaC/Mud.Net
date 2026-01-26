namespace Mud.DataStructures.Flags;

public class Flags : IFlags<string>
{
    private readonly HashSet<string> _hashSet;

    public Flags()
    {
        _hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
    }

    public Flags(params string[] flags)
        : this()
    {
        foreach (var flag in flags)
            Set(flag);
    }

    #region IFlags

    public bool IsNone => _hashSet.Count == 0;

    public bool IsSet(string flag)
        => _hashSet.Contains(flag);

    public bool HasAny(IFlags<string> flags)
        => flags.Values.Any(_hashSet.Contains);

    public bool HasAny(params string[] flags)
        => flags.Any(_hashSet.Contains);

    public bool HasAny(IEnumerable<string> flags)
        => flags.Any(_hashSet.Contains);

    public bool HasAll(IFlags<string> flags)
        => flags.Values.All(_hashSet.Contains);

    public bool HasAll(params string[] flags)
        => flags.All(_hashSet.Contains);

    public bool HasAll(IEnumerable<string> flags)
        => flags.All(_hashSet.Contains);

    public void Set(IFlags<string> flags)
    {
        if (flags != null)
        {
            foreach (var flag in flags.Values)
                _hashSet.Add(flag.Trim());
        }
    }

    public void Set(string flags)
    {
        if (string.IsNullOrWhiteSpace(flags))
            return;
        Set(flags.Split(','));
    }

    public void Set(params string[] flags)
    {
        if (flags != null)
        {
            foreach (string flag in flags)
                _hashSet.Add(flag.Trim());
        }
    }

    public void Unset(IFlags<string> flags)
    {
        if (flags != null)
        {
            foreach (string flag in flags.Values)
                _hashSet.Remove(flag.Trim());
        }
    }

    public void Unset(string flags)
    {
        if (string.IsNullOrWhiteSpace(flags))
            return;
        Unset(flags.Split(','));
    }

    public void Unset(params string[] flags)
    {
        foreach (string flag in flags)
            _hashSet.Remove(flag.Trim());
    }

    public void Copy(IFlags<string> flags)
    {
        _hashSet.Clear();
        if (flags != null)
            Set(flags);
    }

    public int Count => _hashSet.Count;

    public IEnumerable<string> Values => _hashSet;

    public string Serialize()
        => string.Join(',', _hashSet);

    #endregion

    public static bool TryParse(string s, out Flags flags) // TryParse never fails :p
    {
        flags = new Flags();
        if (string.IsNullOrWhiteSpace(s))
            return true;

        flags.Set(s.Split(','));
        return true;
    }

    public static Flags Parse(string s)
    {
        Flags flags = new();
        if (string.IsNullOrWhiteSpace(s))
            return flags;

        flags.Set(s.Split(','));
        return flags;
    }

    public override string ToString()
        => string.Join(", ", _hashSet.OrderBy(x => x));
}