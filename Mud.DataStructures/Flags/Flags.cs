using Mud.Common;

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
        Set(flags);
    }

    #region IFlags

    public bool IsNone => _hashSet.Count == 0;

    public bool IsSet(string flag) => _hashSet.Contains(flag);

    public bool HasAny(params string[] flags) => flags.Any(x => _hashSet.Contains(x));

    public bool HasAll(params string[] flags) => flags.All(x => _hashSet.Contains(x));

    public void Set(string flag) => _hashSet.Add(flag);
    public void Set(params string[] flags)
    {
        foreach (string flag in flags)
            _hashSet.Add(flag);
    }

    public void Unset(string flag) => _hashSet.Remove(flag);
    public void Unset(params string[] flags)
    {
        foreach (string flag in flags)
            _hashSet.Remove(flag);
    }

    public int Count => _hashSet.Count;

    public IEnumerable<string> Items => _hashSet;

    #endregion

    public bool HasAny(Flags flags) => flags.Items.Any(x => _hashSet.Contains(x));

    public bool HasAll(Flags flags) => flags.Items.All(x => _hashSet.Contains(x));

    public void Set(Flags flags)
    {
        _hashSet.UnionWith(flags.Items);
    }

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
}

public abstract class Flags<TFlagValues> : IFlags<string, TFlagValues>
    where TFlagValues : IFlagValues<string>
{
    protected TFlagValues FlagValues { get; }

    private readonly HashSet<string> _hashSet = new(StringComparer.InvariantCultureIgnoreCase);

    protected Flags(TFlagValues flagValues)
    {
        FlagValues = flagValues;
    }

    #region IFlags

    public void Copy(IFlags<string, TFlagValues> flags)
    {
        _hashSet.Clear();
        if (flags != null)
            Set(flags);
    }

    public bool IsNone => _hashSet.Count == 0;

    public bool IsSet(string flag)
    {
        if (!CheckValues(flag))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.IsSet, UnknownValues(flag.Yield()));
        return _hashSet.Contains(flag);
    }

    public bool HasAny(params string[] flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAny, UnknownValues(flags));
        return flags.Any(x => _hashSet.Contains(x));
    }
 
    public bool HasAny(IFlags<string, TFlagValues> flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAny, UnknownValues(flags.Values));
        return flags.Values.Any(x => _hashSet.Contains(x));
    }

    public bool HasAll(params string[] flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAll, UnknownValues(flags));
        return flags.All(x => _hashSet.Contains(x));
    }

    public bool HasAll(IFlags<string, TFlagValues> flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.HasAll, UnknownValues(flags.Values));
        return flags.Values.All(x => _hashSet.Contains(x));
    }

    public void Set(string flags)
    {
        if (string.IsNullOrWhiteSpace(flags))
            return;
        Set(flags.Split(','));
    }

    public void Set(params string[] flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.Set, UnknownValues(flags));
        foreach (var flag in flags)
        {
            if (!string.IsNullOrWhiteSpace(flag))
                _hashSet.Add(flag);
        }
    }

    public void Set(IFlags<string, TFlagValues> flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.Set, UnknownValues(flags.Values));
        foreach (string flag in flags.Values)
            _hashSet.Add(flag);
    }

    public void Unset(string flag)
    {
        if (!CheckValues(flag))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.UnSet, UnknownValues(flag.Yield()));
        _hashSet.Remove(flag);
    }

    public void Unset(params string[] flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.UnSet, UnknownValues(flags));
        foreach (string flag in flags)
            _hashSet.Remove(flag);
    }

    public void Unset(IFlags<string, TFlagValues> flags)
    {
        if (!CheckValues(flags))
            FlagValues.OnUnknownValues(UnknownFlagValueContext.UnSet, UnknownValues(flags.Values));
        foreach (string flag in flags.Values)
            _hashSet.Remove(flag);
    }

    public int Count => _hashSet.Count;

    public IEnumerable<string> Values => _hashSet;

    #endregion

    public override string ToString() => string.Join(", ", _hashSet.OrderBy(x => x));

    private bool CheckValues(params string[] flags) => flags.All(x => FlagValues.AvailableValues.Contains(x));
    private bool CheckValues(IFlags<string, TFlagValues> flags) => flags.Values.All(x => FlagValues.AvailableValues.Contains(x));
    private IEnumerable<string> UnknownValues(IEnumerable<string> flags) => flags.Where(x => !FlagValues.AvailableValues.Contains(x));
}
